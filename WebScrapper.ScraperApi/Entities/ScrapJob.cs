using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapper.ScraperApi.Entities;

[BsonIgnoreExtraElements]
public class ScrapJob
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)]
    public string WebsiteMetadataId { get; set; } = "";

    public string Name { get; set; } = "";
    public string SearchValue { get; set; } = "";
    public List<string> MustContainList { get; set; } = [];
    public List<string> MustNotContainList { get; set; } = [];
    public List<string> MustOrContainList { get; set; } = [];
    public decimal MaxPrice { get; set; }
    public int MaxPages { get; set; } = 1;
    public string? TelegramChatId { get; set; }
    public List<NotificationReceiver> NotificationReceivers { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }

    /// <summary>Populated only by list aggregation. Repo clears before writes.</summary>
    [BsonIgnoreIfNull]
    public string? WebsiteName { get; set; }
}
