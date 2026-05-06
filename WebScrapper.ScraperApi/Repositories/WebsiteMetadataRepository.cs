using MongoDB.Driver;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class WebsiteMetadataRepository : IWebsiteMetadataRepository
{
    private readonly ILogger _logger;

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<WebsiteMetadata> _collection;

    public WebsiteMetadataRepository(ILogger<WebsiteMetadataRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<WebsiteMetadata>("WebsiteMetadatas");
    }

    public async Task<List<WebsiteMetadata>> GetAllAsync()
    {
        try
        {
            return await _collection.Find(Builders<WebsiteMetadata>.Filter.Empty)
                .SortBy(x => x.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            return [];
        }
    }

    public async Task<WebsiteMetadata> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<WebsiteMetadata>.Filter.Eq(x => x.Id, id);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            if (result is null)
                throw new InvalidOperationException($"WebsiteMetadata with ID {id} not found in collection.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task AddAsync(WebsiteMetadata websiteMetadata)
    {
        try
        {
            await _collection.InsertOneAsync(websiteMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
        }
    }

    public async Task UpdateAsync(string id, WebsiteMetadata websiteMetadata)
    {
        try
        {
            websiteMetadata.Id = id;
            var filter = Builders<WebsiteMetadata>.Filter.Eq(x => x.Id, id);
            await _collection.ReplaceOneAsync(filter, websiteMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var filter = Builders<WebsiteMetadata>.Filter.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }
}
