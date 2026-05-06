using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface IScrapRunsService
{
    Task<PagedResult<ScrapRun>> GetPagedAsync(int page, int pageSize);
    Task<ScrapRun?> GetByIdAsync(string id);

    Task<ScrapRun> RunAsync(string scrapJobId);

    /// <summary>
    /// Dry-run: scrapes and evaluates the supplied job (no DB lookup of it),
    /// broadcasting per-ad decisions to the given SignalR stream group, but does
    /// NOT persist ads, send notifications, or write a ScrapRun row. Returns the
    /// in-memory run summary.
    /// </summary>
    Task<ScrapRun> TestAsync(ScrapJob scrapJob, string streamId);
}
