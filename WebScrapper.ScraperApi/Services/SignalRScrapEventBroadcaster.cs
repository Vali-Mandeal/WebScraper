using Microsoft.AspNetCore.SignalR;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Hubs;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class SignalRScrapEventBroadcaster : IScrapEventBroadcaster
{
    private readonly IHubContext<ScrapEventsHub> _hub;

    public SignalRScrapEventBroadcaster(IHubContext<ScrapEventsHub> hub)
    {
        _hub = hub;
    }

    public Task RunStartedAsync(string streamId, ScrapJob scrapJob)
    {
        return _hub.Clients.Group(streamId).SendAsync("RunStarted", new
        {
            jobId = scrapJob.Id,
            jobName = scrapJob.Name,
            mustContain = scrapJob.MustContainList,
            mustNotContain = scrapJob.MustNotContainList,
            maxPrice = scrapJob.MaxPrice,
            startedAt = DateTime.UtcNow
        });
    }

    public Task AdsScrapedAsync(string streamId, int total)
    {
        return _hub.Clients.Group(streamId).SendAsync("AdsScraped", new { total });
    }

    public Task AdDecidedAsync(string streamId, AdDecision decision)
    {
        return _hub.Clients.Group(streamId).SendAsync("AdDecided", new
        {
            ad = new
            {
                id = decision.Ad.Id,
                title = decision.Ad.Title,
                price = decision.Ad.Price,
                locationAndDate = decision.Ad.LocationAndDate,
                url = decision.Ad.Url,
                thumbnailUrl = decision.Ad.ThumbnailUrl
            },
            verdict = decision.Verdict.ToString(),
            reasonCode = decision.ReasonCode,
            reasonArgs = decision.ReasonArgs
        });
    }

    public Task RunFinishedAsync(string streamId, ScrapRun summary)
    {
        return _hub.Clients.Group(streamId).SendAsync("RunFinished", new
        {
            runId = summary.RunId,
            totalScraped = summary.TotalScraped,
            newAdsFound = summary.NewAdsFound,
            notifyWorthyCount = summary.NotifyWorthyCount,
            durationMs = (long)(summary.FinishedAt - summary.StartedAt).TotalMilliseconds,
            status = summary.Status
        });
    }

    public Task RunFailedAsync(string streamId, string error)
    {
        return _hub.Clients.Group(streamId).SendAsync("RunFailed", new { error });
    }
}
