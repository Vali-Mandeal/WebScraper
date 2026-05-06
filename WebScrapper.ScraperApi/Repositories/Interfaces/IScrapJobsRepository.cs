using System.Text.Json;
using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Repositories.Interfaces;

public interface IScrapJobsRepository
{
    Task<List<ScrapJob>> GetAllAsync(bool? active = null);
    Task<ScrapJob> GetByIdAsync(string id);
    Task AddAsync(ScrapJob scrapJob);
    Task UpdateAsync(string id, ScrapJob scrapJob);
    Task PatchAsync(string id, JsonElement updates);
    Task DeleteAsync(string id);
}
