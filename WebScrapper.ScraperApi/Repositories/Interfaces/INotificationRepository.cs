using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Repositories.Interfaces;

public interface INotificationRepository
{
    Task SendNotificationAsync(Notification notification);
}
