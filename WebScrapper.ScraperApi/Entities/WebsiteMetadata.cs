using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScrapper.ScraperApi.Entities;

[BsonIgnoreExtraElements]
public class WebsiteMetadata
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    public bool ShouldAcceptTermsAndConditions { get; set; }
    public bool ShouldScrollToBottom { get; set; }
    public bool ShouldSearch { get; set; }

    public MetadataSelectors Selectors { get; set; } = new();
}

[BsonIgnoreExtraElements]
public class MetadataSelectors
{
    public string TermsAndConditionsButtonSelector { get; set; } = "";
    public string SearchSelector { get; set; } = "";

    public string ScrollToButtonCommand { get; set; } = "";

    public string CardsSelector { get; set; } = "";
    public string CardTitleSelector { get; set; } = "";
    public string CardPriceSelector { get; set; } = "";
    public string LocationAndDateSelector { get; set; } = "";
    public string AdUrlWrapperSelector { get; set; } = "";
    public string AdUrlSelector { get; set; } = "";

    public string ThumbnailUrlWrapperSelector { get; set; } = "";
    public string ThumbnailUrlSelector { get; set; } = "";

    public string BackupThumbnailUrlWrapperSelector { get; set; } = "";
    public string BackupThumbnailUrlSelector { get; set; } = "";

    public string NextPageButtonSelector { get; set; } = "";
}
