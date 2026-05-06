using System.Text.Json;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class ScrapJobsService : IScrapJobsService
{
    private readonly IScrapJobsRepository _scrapJobsRepository;

    public ScrapJobsService(IScrapJobsRepository scrapJobsRepository)
    {
        _scrapJobsRepository = scrapJobsRepository;
    }

    public async Task<List<ScrapJob>> GetAllAsync(bool? active = null)
    {
        return await _scrapJobsRepository.GetAllAsync(active);
    }

    public async Task<ScrapJob> GetByIdAsync(string id)
    {
        return await _scrapJobsRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(ScrapJob scrapJob)
    {
        await _scrapJobsRepository.AddAsync(scrapJob);
    }

    public async Task UpdateAsync(string id, ScrapJob scrapJob)
    {
        await _scrapJobsRepository.UpdateAsync(id, scrapJob);
    }

    public async Task PatchAsync(string id, JsonElement updates)
    {
        await _scrapJobsRepository.PatchAsync(id, updates);
    }

    public async Task DeleteAsync(string id)
    {
        await _scrapJobsRepository.DeleteAsync(id);
    }
}
