using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;

namespace WebScrapper.ScraperApi.Repositories.Interfaces;

public interface IScrapRunsRepository
{
    Task<PagedResult<ScrapRun>> GetPagedAsync(int page, int pageSize);
    Task<ScrapRun?> GetByIdAsync(string id);
    Task AddAsync(ScrapRun scrapRun);
}
