using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(List<Ad> ads, ScrapJob scrapJob);
}
