using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface IScrapEventBroadcaster
{
    Task RunStartedAsync(string streamId, ScrapJob scrapJob);
    Task AdsScrapedAsync(string streamId, int total);
    Task AdDecidedAsync(string streamId, AdDecision decision);
    Task RunFinishedAsync(string streamId, ScrapRun summary);
    Task RunFailedAsync(string streamId, string error);
}
