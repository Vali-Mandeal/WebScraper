using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class ScrapRunsService : IScrapRunsService
{
    private readonly ILogger _logger;
    private readonly IScrapJobsService _scrapJobsService;
    private readonly IWebsiteMetadataService _websiteMetadataService;
    private readonly IScrapService _scrapService;
    private readonly IAdsService _adsService;
    private readonly INotificationService _notificationService;
    private readonly IScrapRunsRepository _scrapRunsRepository;
    private readonly IScrapEventBroadcaster _broadcaster;

    public ScrapRunsService(
        ILogger<ScrapRunsService> logger,
        IScrapJobsService scrapJobsService,
        IWebsiteMetadataService websiteMetadataService,
        IScrapService scrapService,
        IAdsService adsService,
        INotificationService notificationService,
        IScrapRunsRepository scrapRunsRepository,
        IScrapEventBroadcaster broadcaster)
    {
        _logger = logger;
        _scrapJobsService = scrapJobsService;
        _websiteMetadataService = websiteMetadataService;
        _scrapService = scrapService;
        _adsService = adsService;
        _notificationService = notificationService;
        _scrapRunsRepository = scrapRunsRepository;
        _broadcaster = broadcaster;
    }

    public async Task<PagedResult<ScrapRun>> GetPagedAsync(int page, int pageSize)
    {
        return await _scrapRunsRepository.GetPagedAsync(page, pageSize);
    }

    public async Task<ScrapRun?> GetByIdAsync(string id)
    {
        return await _scrapRunsRepository.GetByIdAsync(id);
    }

    public async Task<ScrapRun> RunAsync(string scrapJobId)
    {
        var startedAt = DateTime.UtcNow;
        var runId = "run_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string jobName = "";

        try
        {
            var scrapJob = await _scrapJobsService.GetByIdAsync(scrapJobId);
            jobName = scrapJob.Name;

            var websiteMetadata = await _websiteMetadataService.GetByIdAsync(scrapJob.WebsiteMetadataId);
            var currentAds = await _scrapService.GetCurrentAdsFromWebsiteAsync(scrapJob, websiteMetadata);
            var decisions = await _adsService.EvaluateAsync(currentAds, scrapJob);

            var newAds = decisions
                .Where(d => d.Verdict != AdVerdict.Skipped)
                .Select(d => d.Ad)
                .ToList();

            if (newAds.Any())
                await _adsService.AddAsync(newAds);

            var notificationWorthyAds = decisions
                .Where(d => d.Verdict == AdVerdict.NotifyWorthy)
                .Select(d => d.Ad)
                .ToList();

            await _notificationService.SendNotificationAsync(notificationWorthyAds, scrapJob);

            _logger.LogInformation("Job {Name} finished at: {Now}", scrapJob.Name, DateTime.UtcNow);

            var run = BuildSuccessRun(runId, startedAt, scrapJob, currentAds, decisions);
            await _scrapRunsRepository.AddAsync(run);
            return run;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Run failed for job {Id}: {Message}", scrapJobId, ex.Message);

            var run = BuildFailureRun(runId, startedAt, scrapJobId, jobName, ex);
            await _scrapRunsRepository.AddAsync(run);
            return run;
        }
    }

    private static ScrapRun BuildSuccessRun(string runId, DateTime startedAt, ScrapJob scrapJob, List<Ad> currentAds, List<AdDecision> decisions)
    {
        return new ScrapRun
        {
            RunId = runId,
            ScrapJobId = scrapJob.Id,
            ScrapJobName = scrapJob.Name,
            StartedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            TotalScraped = currentAds.Count,
            NewAdsFound = decisions.Count(d => d.Verdict != AdVerdict.Skipped),
            NotifyWorthyCount = decisions.Count(d => d.Verdict == AdVerdict.NotifyWorthy),
            Status = "success",
            Decisions = decisions.Select(ToLog).ToList()
        };
    }

    private static ScrapRun BuildFailureRun(string runId, DateTime startedAt, string scrapJobId, string jobName, Exception ex)
    {
        return new ScrapRun
        {
            RunId = runId,
            ScrapJobId = scrapJobId,
            ScrapJobName = jobName,
            StartedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            Status = "failure",
            Error = ex.Message
        };
    }

    public async Task<ScrapRun> TestAsync(ScrapJob scrapJob, string streamId)
    {
        var startedAt = DateTime.UtcNow;
        var runId = "test_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        try
        {
            await _broadcaster.RunStartedAsync(streamId, scrapJob);

            var websiteMetadata = await _websiteMetadataService.GetByIdAsync(scrapJob.WebsiteMetadataId);

            // Streaming evaluator: each card produces a decision broadcast immediately
            // as Playwright extracts it. State (existing IDs, dedupe within run) lives
            // in the closure.
            var existingIds = await _adsService.GetExistingAdIdsAsync(scrapJob);
            var seenIds = new HashSet<int>();
            var decisions = new List<AdDecision>();

            var currentAds = await _scrapService.GetCurrentAdsFromWebsiteAsync(
                scrapJob,
                websiteMetadata,
                onAdScraped: async ad =>
                {
                    AdsService.AssignScrapJobMetadata(ad, scrapJob);

                    if (!seenIds.Add(ad.Id)) return;

                    var decision = AdsService.EvaluateOne(ad, scrapJob, existingIds);
                    decisions.Add(decision);
                    await _broadcaster.AdDecidedAsync(streamId, decision);
                });

            await _broadcaster.AdsScrapedAsync(streamId, currentAds.Count);

            var run = BuildSuccessRun(runId, startedAt, scrapJob, currentAds, decisions);
            await _broadcaster.RunFinishedAsync(streamId, run);

            _logger.LogInformation("Test run {RunId} for job {Name} finished", runId, scrapJob.Name);
            return run;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test run failed for job {Name}: {Message}", scrapJob.Name, ex.Message);
            await _broadcaster.RunFailedAsync(streamId, ex.Message);
            return BuildFailureRun(runId, startedAt, scrapJob.Id, scrapJob.Name, ex);
        }
    }

    private static AdDecisionLog ToLog(AdDecision decision)
    {
        return new AdDecisionLog
        {
            AdId = decision.Ad.Id,
            Title = decision.Ad.Title,
            Price = decision.Ad.Price,
            Url = decision.Ad.Url,
            ThumbnailUrl = decision.Ad.ThumbnailUrl,
            Verdict = decision.Verdict,
            ReasonCode = decision.ReasonCode,
            ReasonArgs = decision.ReasonArgs
        };
    }
}
