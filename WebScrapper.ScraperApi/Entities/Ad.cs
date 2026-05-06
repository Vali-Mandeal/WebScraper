using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapper.ScraperApi.Entities;

[BsonIgnoreExtraElements]
public class Ad
{
    [BsonId]
    public int Id { get; set; }

    public string ScrapJobId { get; set; } = "";
    public string? RunId { get; set; }
    public string? Title { get; set; }
    public string? Price { get; set; }
    public string? LocationAndDate { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool ShouldSendNotification { get; set; }
    public bool NotificationSent { get; set; }
    public DateTime SeenAt { get; set; }
}
