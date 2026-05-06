using MongoDB.Driver;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class AdsRepository : IAdsRepository
{
    private readonly ILogger _logger;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Ad> _collection;

    public AdsRepository(ILogger<AdsRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<Ad>("Ads");
    }

    public async Task<List<Ad>> GetByScrapJobIdAsync(string scrapJobId)
    {
        try
        {
            var filter = Builders<Ad>.Filter.Eq(x => x.ScrapJobId, scrapJobId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Ad?> GetByIdAsync(int id)
    {
        try
        {
            var filter = Builders<Ad>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<PagedResult<Ad>> GetPagedAsync(string? scrapJobId, bool? shouldSendNotification, int page, int pageSize)
    {
        try
        {
            var filterBuilder = Builders<Ad>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(scrapJobId))
                filter &= filterBuilder.Eq(x => x.ScrapJobId, scrapJobId);

            if (shouldSendNotification.HasValue)
                filter &= filterBuilder.Eq(x => x.ShouldSendNotification, shouldSendNotification.Value);

            var totalTask = _collection.CountDocumentsAsync(filter);
            var dataTask = _collection
                .Find(filter)
                .SortByDescending(x => x.SeenAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            await Task.WhenAll(totalTask, dataTask);

            return new PagedResult<Ad>
            {
                Data = dataTask.Result,
                Total = totalTask.Result,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task AddAsync(List<Ad> ads)
    {
        try
        {
            await _collection.InsertManyAsync(ads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }
}
