using Quartz;
using WebScrapper.ScraperApi.BackgroundJobs;
using WebScrapper.ScraperApi.Endpoints;
using WebScrapper.ScraperApi.Extensions;
using WebScrapper.ScraperApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddScraperServices(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials());
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("HourlyScrape");
    q.AddJob<HourlyScrape>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("HourlyScrape-trigger")
        .WithCronSchedule("0 0 9-22 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest"))));
});

builder.Services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthEndpoints();
app.MapScrapJobsEndpoints();
app.MapWebsitesEndpoints();
app.MapListingsEndpoints();
app.MapScrapRunsEndpoints();

app.MapHub<ScrapEventsHub>("/hubs/scrap-events");

app.Run();
