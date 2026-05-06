using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Models;

/// <summary>
/// Test runs carry the full ScrapJob so the caller can iterate on filters/price
/// without persisting between attempts. The Job's WebsiteMetadataId is still
/// resolved from the DB; everything else uses the values in this request.
/// </summary>
public record TestScrapeRequest(ScrapJob ScrapJob, string StreamId);
