using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger _logger;
    private readonly IEnumerable<INotificationRepository> _notificationRepositories;

    public NotificationService(ILogger<NotificationService> logger, IEnumerable<INotificationRepository> notificationRepositories)
    {
        _logger = logger;
        _notificationRepositories = notificationRepositories;
    }

    public async Task SendNotificationAsync(List<Ad> ads, ScrapJob scrapJob)
    {
        if (ads.Any() == false)
        {
            _logger.LogInformation($"No new ads for {scrapJob.Name}");
            return;
        }

        _logger.LogInformation($"Generating notification for job: {scrapJob.Name}, found {ads.Count} ads.");

        var notification = new Notification(ads, scrapJob);

        foreach (var repository in _notificationRepositories)
            await repository.SendNotificationAsync(notification);
    }
}
