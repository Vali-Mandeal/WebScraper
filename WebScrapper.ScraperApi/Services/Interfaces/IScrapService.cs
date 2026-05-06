using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface IScrapService
{
    /// <summary>
    /// Scrapes ads from the website. If <paramref name="onAdScraped"/> is provided,
    /// it is invoked for each card as soon as it's extracted (for live test streaming).
    /// </summary>
    Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(
        ScrapJob scrapJob,
        WebsiteMetadata websiteMetadata,
        Func<Ad, Task>? onAdScraped = null);
}
