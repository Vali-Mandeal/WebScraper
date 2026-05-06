using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Endpoints;

public static class WebsitesEndpoints
{
    public static IEndpointRouteBuilder MapWebsitesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/websites", async (IWebsiteMetadataService websiteMetadataService) =>
            Results.Ok(await websiteMetadataService.GetAllAsync()));

        app.MapGet("/websites/{id}", async (string id, IWebsiteMetadataService websiteMetadataService) =>
            Results.Ok(await websiteMetadataService.GetByIdAsync(id)));

        app.MapPost("/websites", async (WebsiteMetadata websiteMetadata, IWebsiteMetadataService websiteMetadataService) =>
        {
            await websiteMetadataService.AddAsync(websiteMetadata);
            return Results.Created($"/websites/{websiteMetadata.Id}", websiteMetadata);
        });

        app.MapPut("/websites/{id}", async (string id, WebsiteMetadata websiteMetadata, IWebsiteMetadataService websiteMetadataService) =>
        {
            await websiteMetadataService.UpdateAsync(id, websiteMetadata);
            return Results.NoContent();
        });

        app.MapDelete("/websites/{id}", async (string id, IWebsiteMetadataService websiteMetadataService) =>
        {
            await websiteMetadataService.DeleteAsync(id);
            return Results.NoContent();
        });

        return app;
    }
}
