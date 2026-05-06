using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapper.ScraperApi.Entities;

[BsonIgnoreExtraElements]
public class ScrapRun
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    public string RunId { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)]
    public string ScrapJobId { get; set; } = "";

    public string ScrapJobName { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public int TotalScraped { get; set; }
    public int NewAdsFound { get; set; }
    public int NotifyWorthyCount { get; set; }
    public string Status { get; set; } = "";
    public string? Error { get; set; }
    public List<AdDecisionLog> Decisions { get; set; } = [];
}
