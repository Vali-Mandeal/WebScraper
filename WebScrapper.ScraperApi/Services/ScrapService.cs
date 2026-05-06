using Microsoft.Playwright;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class ScrapService : IScrapService
{
    private readonly ILogger _logger;

    private const bool _headless = true;

    public ScrapService(ILogger<ScrapService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Ad>> GetCurrentAdsFromWebsiteAsync(
        ScrapJob scrapJob,
        WebsiteMetadata websiteMetadata,
        Func<Ad, Task>? onAdScraped = null)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await GetBrowserAsync(playwright);
        IPage page = await GetPageAsync(browser);

        await LoadPageAsync(page, scrapJob, websiteMetadata);
        await AcceptTermsAndConditionsAsync(page, websiteMetadata);
        await SearchAsync(page, scrapJob.SearchValue, websiteMetadata);

        var ads = new List<Ad>();
        var maxPages = Math.Max(1, scrapJob.MaxPages);

        for (int pageIndex = 0; pageIndex < maxPages; pageIndex++)
        {
            _logger.LogInformation("Scraping page {Page}/{Total}", pageIndex + 1, maxPages);

            await ScrollPageToBottomAsync(page, websiteMetadata);

            var cards = await GetCardAdsAsync(page, websiteMetadata);
            var pageAds = await ExtractAdsFromCardsAsync(scrapJob, cards, websiteMetadata, onAdScraped);
            ads.AddRange(pageAds);

            if (pageIndex >= maxPages - 1) break;

            var advanced = await GoToNextPageAsync(page, websiteMetadata);
            if (!advanced) break;
        }

        return ads;
    }

    private async Task<bool> GoToNextPageAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        var selector = websiteMetadata.Selectors.NextPageButtonSelector;
        if (string.IsNullOrWhiteSpace(selector))
            return false;

        try
        {
            var nextButton = page.Locator(selector).First;
            await nextButton.ScrollIntoViewIfNeededAsync(new LocatorScrollIntoViewIfNeededOptions { Timeout = 10000 });
            await nextButton.ClickAsync(new LocatorClickOptions { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            _logger.LogInformation("Navigated to next page: {Url}", page.Url);
            return true;
        }
        catch (System.TimeoutException)
        {
            _logger.LogInformation("Next page button not found, ending pagination.");
            return false;
        }
    }

    private async Task<IBrowser> GetBrowserAsync(IPlaywright playwright)
    {
        _logger.LogInformation("Launching browser");
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _headless,
            Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
        });

        return browser;
    }

    private async static Task<IPage> GetPageAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                        "(KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });
        var page = await context.NewPageAsync();

        return page;
    }

    private async Task LoadPageAsync(IPage page, ScrapJob scrapJob, WebsiteMetadata websiteMetadata)
    {
        _logger.LogInformation($"Loading page: {websiteMetadata.Url}");
        await page.GotoAsync(websiteMetadata.Url);
    }

    private async Task AcceptTermsAndConditionsAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldAcceptTermsAndConditions is false)
            return;

        try
        {
            var acceptButton = page.Locator(websiteMetadata.Selectors.TermsAndConditionsButtonSelector);
            await acceptButton.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
            await acceptButton.ClickAsync();

            _logger.LogInformation("Accepted ToC.");
        }
        catch (System.TimeoutException)
        {
            _logger.LogWarning("ToC button did not appear, moving on.");
        }
    }

    private async Task SearchAsync(IPage page, string searchValue, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldSearch is false)
            return;

        _logger.LogInformation($"Using search value: {searchValue}");
        var searchBox = page.Locator(websiteMetadata.Selectors.SearchSelector);
        await searchBox.WaitForAsync();
        await searchBox.FillAsync(searchValue);

        await searchBox.PressAsync("Enter");
    }

    private async Task ScrollPageToBottomAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        if (websiteMetadata.ShouldScrollToBottom is false)
            return;

        _logger.LogInformation("Scrolling to bottom of page started.");

        var nextPageSelector = websiteMetadata.Selectors.NextPageButtonSelector;
        var hasPaginationHint = !string.IsNullOrWhiteSpace(nextPageSelector);

        for (int i = 0; i < 50; i++)
        {
            await page.EvaluateAsync(websiteMetadata.Selectors.ScrollToButtonCommand);
            await Task.Delay(500);

            if (hasPaginationHint && await page.Locator(nextPageSelector).First.IsVisibleAsync())
            {
                _logger.LogInformation("Reached pagination after {Iter} scroll(s).", i + 1);
                return;
            }
        }

        _logger.LogInformation("Scrolling to bottom of page done (max iterations).");
    }

    private async Task<IReadOnlyList<IElementHandle>> GetCardAdsAsync(IPage page, WebsiteMetadata websiteMetadata)
    {
        _logger.LogInformation("Reading card ads.");
        var cardSelector = websiteMetadata.Selectors.CardsSelector;

        await Task.Delay(3000);

        await page.WaitForSelectorAsync(cardSelector);

        var cards = await page.QuerySelectorAllAsync(cardSelector);

        return cards;
    }

    private async Task<List<Ad>> ExtractAdsFromCardsAsync(ScrapJob scrapJob, IReadOnlyList<IElementHandle> cards, WebsiteMetadata websiteMetadata, Func<Ad, Task>? onAdScraped)
    {
        _logger.LogInformation("Extracting card ads.");
        var ads = new List<Ad>();

        foreach (var card in cards)
        {
            Ad ad = await GetAdAsync(card, scrapJob, websiteMetadata);

            ads.Add(ad);

            if (onAdScraped is not null)
                await onAdScraped(ad);
        }

        return ads;
    }

    private static async Task<Ad> GetAdAsync(IElementHandle card, ScrapJob scrapJob, WebsiteMetadata websiteMetadata)
    {
        var selectors = websiteMetadata.Selectors;

        var ad = new Ad();

        var titleElement = await card.QuerySelectorAsync(selectors.CardTitleSelector);
        ad.Title = titleElement is not null
            ? await titleElement.InnerTextAsync()
            : null;

        var priceElement = await card.QuerySelectorAsync(selectors.CardPriceSelector);
        ad.Price = priceElement is not null
            ? await priceElement.InnerTextAsync()
            : null;

        var locationDateElement = await card.QuerySelectorAsync(selectors.LocationAndDateSelector);
        ad.LocationAndDate = locationDateElement is not null
            ? await locationDateElement.InnerTextAsync()
            : null;

        var adUrlElement = await card.QuerySelectorAsync(selectors.AdUrlWrapperSelector);
        var partialUrl = adUrlElement is not null ? await adUrlElement.GetAttributeAsync(selectors.AdUrlSelector) : null;
        ad.Url = partialUrl is not null
            ? websiteMetadata.Url + partialUrl
            : null;

        var thumbnailElement = await card.QuerySelectorAsync(selectors.ThumbnailUrlWrapperSelector);
        ad.ThumbnailUrl = thumbnailElement is not null
            ? await thumbnailElement.GetAttributeAsync(selectors.ThumbnailUrlSelector)
            : null;

        if (ad.ThumbnailUrl is null)
        {
            thumbnailElement = await card.QuerySelectorAsync(selectors.BackupThumbnailUrlWrapperSelector);

            ad.ThumbnailUrl = thumbnailElement is not null
                ? await thumbnailElement.GetAttributeAsync(selectors.BackupThumbnailUrlSelector)
                : null;
        }

        return ad;
    }
}
