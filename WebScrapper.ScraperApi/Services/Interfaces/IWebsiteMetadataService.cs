using WebScrapper.ScraperApi.Entities;

namespace WebScrapper.ScraperApi.Services.Interfaces;

public interface IWebsiteMetadataService
{
    Task<List<WebsiteMetadata>> GetAllAsync();
    Task<WebsiteMetadata> GetByIdAsync(string id);
    Task AddAsync(WebsiteMetadata websiteMetadata);
    Task UpdateAsync(string id, WebsiteMetadata websiteMetadata);
    Task DeleteAsync(string id);
}
