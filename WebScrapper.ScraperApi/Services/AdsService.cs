using System.Globalization;
using System.Text.RegularExpressions;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Extensions;
using WebScrapper.ScraperApi.Models;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class AdsService : IAdsService
{
    private readonly ILogger _logger;
    private readonly IAdsRepository _adsRepository;

    public AdsService(ILogger<AdsService> logger, IAdsRepository adsRepository)
    {
        _logger = logger;
        _adsRepository = adsRepository;
    }

    public async Task<List<AdDecision>> EvaluateAsync(List<Ad> scrappedAds, ScrapJob scrapJob)
    {
        _logger.LogInformation($"Evaluating {scrappedAds.Count} ads for job {scrapJob.Name}.");

        var existingIds = await GetExistingAdIdsAsync(scrapJob);

        SetScrappedJobsIds(scrappedAds, scrapJob);

        var decisions = new List<AdDecision>();
        var seenIds = new HashSet<int>();

        foreach (var ad in scrappedAds)
        {
            if (!seenIds.Add(ad.Id))
                continue;

            decisions.Add(EvaluateAd(ad, scrapJob, existingIds));
        }

        return decisions;
    }

    public async Task<HashSet<int>> GetExistingAdIdsAsync(ScrapJob scrapJob)
    {
        var existingsAds = await _adsRepository.GetByScrapJobIdAsync(scrapJob.Id);
        return existingsAds.Select(a => a.Id).ToHashSet();
    }

    public static void AssignScrapJobMetadata(Ad ad, ScrapJob scrapJob)
    {
        if (ad.Url == null) return;

        ad.Id = ad.Url.GetId();
        ad.ScrapJobId = scrapJob.Id;
        ad.SeenAt = DateTime.UtcNow;
    }

    public static AdDecision EvaluateOne(Ad ad, ScrapJob scrapJob, HashSet<int> existingIds)
    {
        return EvaluateAd(ad, scrapJob, existingIds);
    }

    public async Task<List<Ad>> GetNewAsync(List<Ad> scrappedAds, ScrapJob scrapJob)
    {
        var decisions = await EvaluateAsync(scrappedAds, scrapJob);

        return decisions
            .Where(d => d.Verdict != AdVerdict.Skipped)
            .Select(d => d.Ad)
            .ToList();
    }

    public async Task<PagedResult<Ad>> GetPagedAsync(string? scrapJobId, bool? shouldSendNotification, int page, int pageSize)
    {
        return await _adsRepository.GetPagedAsync(scrapJobId, shouldSendNotification, page, pageSize);
    }

    public async Task<Ad?> GetByIdAsync(int id)
    {
        return await _adsRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(List<Ad> ads)
    {
        _logger.LogInformation($"Saving new ads. Count: {ads.Count}");

        await _adsRepository.AddAsync(ads);
    }

    private void SetScrappedJobsIds(List<Ad> currentAds, ScrapJob scrapJob)
    {
        foreach (var ad in currentAds)
        {
            if (ad.Url == null)
            {
                _logger.LogError($"Ad url is null. Ad: Id={ad.Id}, Title={ad.Title}, Price={ad.Price}, Location={ad.LocationAndDate}, Scrajobid: {ad.ScrapJobId}");
                continue;
            }

            ad.Id = ad.Url.GetId();
            ad.ScrapJobId = scrapJob.Id;
            ad.SeenAt = DateTime.UtcNow;
        }
    }

    private static AdDecision EvaluateAd(Ad ad, ScrapJob scrapJob, HashSet<int> existingIds)
    {
        if (existingIds.Contains(ad.Id))
            return new AdDecision { Ad = ad, Verdict = AdVerdict.Skipped, ReasonCode = "Duplicate" };

        var missing = scrapJob.MustContainList.FirstOrDefault(kw => !ContainsAsWord(ad.Title, kw));
        if (missing is not null)
            return new AdDecision
            {
                Ad = ad,
                Verdict = AdVerdict.Skipped,
                ReasonCode = "MissingRequiredKeyword",
                ReasonArgs = new() { ["keyword"] = missing }
            };

        var excluded = scrapJob.MustNotContainList.FirstOrDefault(kw => ContainsAsWord(ad.Title, kw));
        if (excluded is not null)
            return new AdDecision
            {
                Ad = ad,
                Verdict = AdVerdict.Skipped,
                ReasonCode = "ExcludedKeyword",
                ReasonArgs = new() { ["keyword"] = excluded }
            };

        var price = TryGetPrice(ad.Price);
        if (price is not null && price > scrapJob.MaxPrice)
        {
            ad.ShouldSendNotification = false;
            return new AdDecision
            {
                Ad = ad,
                Verdict = AdVerdict.SavedSilent,
                ReasonCode = "PriceTooHigh",
                ReasonArgs = new()
                {
                    ["price"] = price.Value.ToString(CultureInfo.InvariantCulture),
                    ["maxPrice"] = scrapJob.MaxPrice.ToString(CultureInfo.InvariantCulture)
                }
            };
        }

        ad.ShouldSendNotification = true;
        return new AdDecision { Ad = ad, Verdict = AdVerdict.NotifyWorthy, ReasonCode = "NotifyWorthy" };
    }

    private static decimal? TryGetPrice(string? price)
    {
        if (string.IsNullOrWhiteSpace(price)) return null;
        try { return price.GetPrice(); }
        catch { return null; }
    }

    private static bool ContainsAsWord(string? title, string keyword)
    {
        // For purely numeric keywords (e.g. "16", "6") word boundaries are useless
        // because a digit run is treated as one "word" — `\b16\b` won't fire inside
        // "16TB" or "16MB". Use a digit boundary instead so adjacent digits block
        // the match (so "6" doesn't match "16") but adjacent letters do not.
        var pattern = keyword.All(char.IsDigit)
            ? $@"(?<!\d){Regex.Escape(keyword)}(?!\d)"
            : $@"\b{Regex.Escape(keyword)}\b";

        return Regex.IsMatch(title ?? "", pattern, RegexOptions.IgnoreCase);
    }
}
