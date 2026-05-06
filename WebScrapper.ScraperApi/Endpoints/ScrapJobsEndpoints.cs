using System.Text.Json;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Endpoints;

public static class ScrapJobsEndpoints
{
    public static IEndpointRouteBuilder MapScrapJobsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/scrapjobs", async (IScrapJobsService scrapJobsService, bool? active) =>
            Results.Ok(await scrapJobsService.GetAllAsync(active)));

        app.MapGet("/scrapjobs/{id}", async (string id, IScrapJobsService scrapJobsService) =>
            Results.Ok(await scrapJobsService.GetByIdAsync(id)));

        app.MapPost("/scrapjobs", async (ScrapJob scrapJob, IScrapJobsService scrapJobsService) =>
        {
            await scrapJobsService.AddAsync(scrapJob);
            return Results.Created($"/scrapjobs/{scrapJob.Id}", scrapJob);
        });

        app.MapPut("/scrapjobs/{id}", async (string id, ScrapJob scrapJob, IScrapJobsService scrapJobsService) =>
        {
            await scrapJobsService.UpdateAsync(id, scrapJob);
            return Results.NoContent();
        });

        app.MapPatch("/scrapjobs/{id}", async (string id, JsonElement updates, IScrapJobsService scrapJobsService) =>
        {
            await scrapJobsService.PatchAsync(id, updates);
            return Results.NoContent();
        });

        app.MapDelete("/scrapjobs/{id}", async (string id, IScrapJobsService scrapJobsService) =>
        {
            await scrapJobsService.DeleteAsync(id);
            return Results.NoContent();
        });

        return app;
    }
}
