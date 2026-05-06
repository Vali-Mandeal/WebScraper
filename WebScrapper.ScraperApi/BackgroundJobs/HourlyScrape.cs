using Quartz;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.BackgroundJobs;

public class HourlyScrape : IJob
{
    private readonly ILogger<HourlyScrape> _logger;
    private readonly IScrapJobsService _scrapJobsService;
    private readonly IScrapRunsService _scrapRunsService;
    
    public HourlyScrape(ILogger<HourlyScrape> logger, IScrapJobsService scrapJobsService, IScrapRunsService scrapRunsService)
    {
        _logger = logger;
        _scrapJobsService = scrapJobsService;
        _scrapRunsService = scrapRunsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Job started at: {DateTime.Now}");

        await ExecuteJobs();
    }

    private async Task ExecuteJobs()
    {
        var activeJobs = await _scrapJobsService.GetAllAsync(active: true);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 3 
        };

        await Parallel.ForEachAsync(
            activeJobs,
            parallelOptions,
            async (activeJob, ct) =>
            {
                _logger.LogInformation($"Running job {activeJob.Name} in parallel");
                
                var result = await _scrapRunsService.RunAsync(activeJob.Id);
                
                var resultString = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                
                _logger.LogInformation($"Finished job with result {resultString}");
            });
    }
}