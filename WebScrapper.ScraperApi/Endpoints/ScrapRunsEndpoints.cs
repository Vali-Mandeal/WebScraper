using WebScrapper.ScraperApi.Models;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Endpoints;

// Request DTOs colocated with the endpoint that consumes them.
public record RunScrapeRequest(string ScrapJobId);

public static class ScrapRunsEndpoints
{
    public static IEndpointRouteBuilder MapScrapRunsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/scrapruns", async (IScrapRunsService scrapRunsService, int? page, int? pageSize) =>
        {
            var p = page is > 0 ? page.Value : 1;
            var ps = pageSize is > 0 and <= 100 ? pageSize.Value : 25;
            return Results.Ok(await scrapRunsService.GetPagedAsync(p, ps));
        });

        app.MapGet("/scrapruns/{id}", async (string id, IScrapRunsService scrapRunsService) =>
        {
            var run = await scrapRunsService.GetByIdAsync(id);
            return run is null ? Results.NotFound() : Results.Ok(run);
        });

        app.MapPost("/scrapruns", async (RunScrapeRequest request, IScrapRunsService scrapRunsService) =>
        {
            var run = await scrapRunsService.RunAsync(request.ScrapJobId);
            return run.Status == "success"
                ? Results.Created($"/scrapruns/{run.Id}", run)
                : Results.Problem(run.Error, statusCode: 500);
        });

        app.MapPost("/scrapruns/test", (TestScrapeRequest request, IServiceScopeFactory scopeFactory) =>
        {
            // Fire-and-forget: server runs the dry-run pipeline in the background,
            // broadcasting decisions to the SignalR group `streamId`. Client subscribes
            // to the hub before posting and listens for "AdDecided" / "RunFinished" / etc.
            // The job in the request body is used as-is (no DB lookup), so callers
            // can iterate on filters/price without having to save first.
            _ = Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IScrapRunsService>();
                await svc.TestAsync(request.ScrapJob, request.StreamId);
            });

            return Results.Accepted(value: new { streamId = request.StreamId });
        });

        return app;
    }
}
