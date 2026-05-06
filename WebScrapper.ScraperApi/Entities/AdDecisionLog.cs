using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapper.ScraperApi.Entities;

[BsonIgnoreExtraElements]
public class AdDecisionLog
{
    public int AdId { get; set; }
    public string? Title { get; set; }
    public string? Price { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public AdVerdict Verdict { get; set; }
    public string ReasonCode { get; set; } = "";
    public Dictionary<string, string> ReasonArgs { get; set; } = new();

    [BsonIgnore]
    public string Human => ReasonCode switch
    {
        "Duplicate"              => "Already saved in a previous run",
        "MissingRequiredKeyword" => $"Missing required keyword '{ReasonArgs.GetValueOrDefault("keyword")}'",
        "ExcludedKeyword"        => $"Contains excluded keyword '{ReasonArgs.GetValueOrDefault("keyword")}'",
        "PriceTooHigh"           => $"Price {ReasonArgs.GetValueOrDefault("price")} exceeds max {ReasonArgs.GetValueOrDefault("maxPrice")}",
        "NotifyWorthy"           => "Matches all filters and price within budget",
        _                         => ReasonCode
    };
}
