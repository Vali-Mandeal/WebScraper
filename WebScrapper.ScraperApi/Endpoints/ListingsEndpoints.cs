using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Endpoints;

// Renamed from "Ads" to dodge ad-blocker URL filters. Domain entity is still Ad.
public static class ListingsEndpoints
{
    public static IEndpointRouteBuilder MapListingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/listings", async (
            IAdsService adsService,
            string? scrapJobId,
            bool? shouldSendNotification,
            int? page,
            int? pageSize) =>
        {
            var p = page is > 0 ? page.Value : 1;
            var ps = pageSize is > 0 and <= 200 ? pageSize.Value : 48;
            return Results.Ok(await adsService.GetPagedAsync(scrapJobId, shouldSendNotification, p, ps));
        });

        app.MapGet("/listings/{id:int}", async (int id, IAdsService adsService) =>
        {
            var ad = await adsService.GetByIdAsync(id);
            return ad is null ? Results.NotFound() : Results.Ok(ad);
        });

        return app;
    }
}
