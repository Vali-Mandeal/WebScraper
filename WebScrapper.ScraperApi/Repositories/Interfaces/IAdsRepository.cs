using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;

namespace WebScrapper.ScraperApi.Repositories.Interfaces;

public interface IAdsRepository
{
    Task<List<Ad>> GetByScrapJobIdAsync(string scrapJobId);
    Task<Ad?> GetByIdAsync(int id);
    Task<PagedResult<Ad>> GetPagedAsync(string? scrapJobId, bool? shouldSendNotification, int page, int pageSize);
    Task AddAsync(List<Ad> ads);
}
