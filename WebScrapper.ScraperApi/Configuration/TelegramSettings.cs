namespace WebScrapper.ScraperApi.Configuration;

public class TelegramSettings
{
    public const string Key = "TelegramSettings";
    public string BotToken { get; set; } = string.Empty;
    public string DefaultChatId { get; set; } = string.Empty;
}
