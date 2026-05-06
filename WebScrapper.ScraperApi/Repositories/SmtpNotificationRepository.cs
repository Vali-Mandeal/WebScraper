using System.Text;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using WebScrapper.ScraperApi.Configuration;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class SmtpNotificationRepository : INotificationRepository, IDisposable
{
    private readonly ILogger _logger;
    private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
    private readonly SmtpSettings _smtpSettings;

    public SmtpNotificationRepository(ILogger<SmtpNotificationRepository> logger, IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _smtpClient = new MailKit.Net.Smtp.SmtpClient();
        _smtpSettings = smtpSettings.Value;

        if (string.IsNullOrEmpty(_smtpSettings.SmtpHost))
            return;

        ConnectAndAuthenticateSmtpClient().GetAwaiter().GetResult();
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        if (notification.Job.NotificationReceivers.Count == 0)
            return;

        var subject = $"New ads for {notification.Job.Name} {DateTime.UtcNow}";
        var htmlBody = GetHtmlBody(notification.Ads);

        foreach (var receiver in notification.Job.NotificationReceivers)
        {
            var message = new MimeMessage();
            SetEmailMetadata(notification.Job.Name, subject, receiver, message);
            SetEmailBody(htmlBody, message);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} for {receiver.Email} due to {exception.Message}");
                });

            await retryPolicy.ExecuteAsync(async () =>
            {
                if (!_smtpClient.IsConnected)
                {
                    await ConnectAndAuthenticateSmtpClient();
                }

                try
                {
                    await _smtpClient.SendAsync(message);
                    _logger.LogInformation($"Notification sent successfully to: {receiver.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending notification to: {receiver.Email}, {ex.Message}");
                }
            });
        }
    }

    private async Task ConnectAndAuthenticateSmtpClient()
    {
        try
        {
            await _smtpClient.ConnectAsync(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort, _smtpSettings.SecureSocketOptions);
            await _smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error connecting/authenticating SMTP client: {ex.Message}");
        }
    }

    private void SetEmailMetadata(string jobName, string subject, NotificationReceiver receiver, MimeMessage message)
    {
        message.From.Add(new MailboxAddress(jobName, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress(receiver.Name, receiver.Email));
        message.Subject = subject;
    }

    private static void SetEmailBody(string htmlBody, MimeMessage message)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();
    }

    private static string GetHtmlBody(List<Ad> ads)
    {
        const string htmlStart = "<html><body>";
        const string htmlEnd = "</body></html>";

        var adsText = new StringBuilder();

        adsText.AppendLine(htmlStart);

        foreach (var ad in ads)
            AddAdDetailsToHtml(adsText, ad);

        adsText.AppendLine(htmlEnd);

        return adsText.ToString();
    }

    private static void AddAdDetailsToHtml(StringBuilder adsText, Ad ad)
    {
        adsText.AppendLine($"<p><strong>{ad.Title}</strong></p>");
        adsText.AppendLine($"<p><strong>Price:</strong> {ad.Price}</p>");
        adsText.AppendLine($"<p><strong>Location and Date:</strong> {ad.LocationAndDate}</p>");
        adsText.AppendLine($"<p><strong>Link:</strong> <a href='{ad.Url}'>{ad.Url}</a></p>");
        adsText.AppendLine($"<p><img src='{ad.ThumbnailUrl}' alt='Thumbnail' /></p>");
        adsText.AppendLine("<hr>");
    }

    public void Dispose()
    {
        if (_smtpClient.IsConnected)
        {
            _smtpClient.Disconnect(true);
        }
        _smtpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
