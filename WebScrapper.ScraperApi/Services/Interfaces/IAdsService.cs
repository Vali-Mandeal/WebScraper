using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface IAdsService
{
    /// <summary>
    /// Evaluates each scrapped ad against the job's filters and price gate, returning a verdict per ad.
    /// Skipped ads will not be persisted by the caller. SavedSilent and NotifyWorthy ads will.
    /// </summary>
    Task<List<AdDecision>> EvaluateAsync(List<Ad> scrappedAds, ScrapJob scrapJob);

    /// <summary>Pre-fetches the set of existing ad IDs for the given job. Used by streaming evaluators.</summary>
    Task<HashSet<int>> GetExistingAdIdsAsync(ScrapJob scrapJob);

    /// <summary>
    /// Identifies the new ads by comparing the <paramref name="scrappedAds"/> against existing ads stored in the database
    /// </summary>
    /// <returns>A list of ads from <paramref name="scrappedAds"/> that do not already exist in the database</returns>
    Task<List<Ad>> GetNewAsync(List<Ad> scrappedAds, ScrapJob scrapJob);

    Task<PagedResult<Ad>> GetPagedAsync(string? scrapJobId, bool? shouldSendNotification, int page, int pageSize);

    Task<Ad?> GetByIdAsync(int id);

    Task AddAsync(List<Ad> ads);
}
