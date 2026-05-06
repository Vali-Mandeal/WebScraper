using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Services;

public class WebsiteMetadataService : IWebsiteMetadataService
{
    private readonly IWebsiteMetadataRepository _websiteMetadataRepository;

    public WebsiteMetadataService(IWebsiteMetadataRepository websiteMetadataRepository)
    {
        _websiteMetadataRepository = websiteMetadataRepository;
    }

    public async Task<List<WebsiteMetadata>> GetAllAsync()
    {
        return await _websiteMetadataRepository.GetAllAsync();
    }

    public async Task<WebsiteMetadata> GetByIdAsync(string id)
    {
        return await _websiteMetadataRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(WebsiteMetadata websiteMetadata)
    {
        await _websiteMetadataRepository.AddAsync(websiteMetadata);
    }

    public async Task UpdateAsync(string id, WebsiteMetadata websiteMetadata)
    {
        await _websiteMetadataRepository.UpdateAsync(id, websiteMetadata);
    }

    public async Task DeleteAsync(string id)
    {
        await _websiteMetadataRepository.DeleteAsync(id);
    }
}
