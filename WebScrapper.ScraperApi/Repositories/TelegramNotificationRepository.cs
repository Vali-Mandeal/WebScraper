using System.Text;
using Microsoft.Extensions.Options;
using WebScrapper.ScraperApi.Configuration;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class TelegramNotificationRepository : INotificationRepository
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly TelegramSettings _telegramSettings;

    public TelegramNotificationRepository(ILogger<TelegramNotificationRepository> logger, HttpClient httpClient, IOptions<TelegramSettings> telegramSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _telegramSettings = telegramSettings.Value;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        var chatId = notification.Job.TelegramChatId ?? _telegramSettings.DefaultChatId;
        if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(_telegramSettings.BotToken))
            return;

        var body = GetTelegramBody(notification.Ads, notification.Job);
        await SendTelegramMessageAsync(chatId, body);
    }

    private async Task SendTelegramMessageAsync(string chatId, string body)
    {
        var url = $"https://api.telegram.org/bot{_telegramSettings.BotToken}/sendMessage";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = chatId,
            ["text"] = body,
            ["parse_mode"] = "HTML",
            ["disable_web_page_preview"] = "true"
        });

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Telegram notification sent successfully to chat: {chatId}");
                return;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Telegram notification failed for chat {chatId}: {response.StatusCode} {error}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending Telegram notification to {chatId}, {ex.Message}");
        }
    }

    private static string GetTelegramBody(List<Ad> ads, ScrapJob scrapJob)
    {
        var lines = new StringBuilder();
        lines.AppendLine($"🔍 <b>{Escape(scrapJob.Name)}</b> - {ads.Count} new ad{(ads.Count > 1 ? "s" : "")}");
        lines.AppendLine();

        foreach (var ad in ads)
            AddAdDetailsToBody(lines, ad);

        return lines.ToString().TrimEnd();
    }

    private static void AddAdDetailsToBody(StringBuilder lines, Ad ad)
    {
        var title = !string.IsNullOrEmpty(ad.Url)
            ? $"<a href=\"{Escape(ad.Url)}\">{Escape(ad.Title ?? "?")}</a>"
            : $"<b>{Escape(ad.Title ?? "?")}</b>";

        lines.AppendLine($"📦 {title}");
        if (!string.IsNullOrEmpty(ad.Price))           lines.AppendLine($"💰 {Escape(ad.Price)}");
        if (!string.IsNullOrEmpty(ad.LocationAndDate)) lines.AppendLine($"📍 {Escape(ad.LocationAndDate)}");
        lines.AppendLine();
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
