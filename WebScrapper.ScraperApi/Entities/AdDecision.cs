namespace WebScrapper.ScraperApi.Entities;

public class AdDecision
{
    public Ad Ad { get; init; } = null!;
    public AdVerdict Verdict { get; init; }
    public string ReasonCode { get; init; } = "";
    public Dictionary<string, string> ReasonArgs { get; init; } = new();
}
