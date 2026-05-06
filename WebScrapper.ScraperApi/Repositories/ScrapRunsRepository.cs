using MongoDB.Driver;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Models;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class ScrapRunsRepository : IScrapRunsRepository
{
    private readonly ILogger _logger;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScrapRun> _collection;

    public ScrapRunsRepository(ILogger<ScrapRunsRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<ScrapRun>("ScrapRuns");
    }

    public async Task<PagedResult<ScrapRun>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            // Filter out stalled rows from previous workflow versions (StartedAt missing/null).
            var filter = Builders<ScrapRun>.Filter.Ne(x => x.StartedAt, default);

            var totalTask = _collection.CountDocumentsAsync(filter);
            var dataTask = _collection
                .Find(filter)
                .SortByDescending(x => x.StartedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project<ScrapRun>(Builders<ScrapRun>.Projection.Exclude(x => x.Decisions))
                .ToListAsync();

            await Task.WhenAll(totalTask, dataTask);

            return new PagedResult<ScrapRun>
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

    public async Task<ScrapRun?> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<ScrapRun>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task AddAsync(ScrapRun scrapRun)
    {
        try
        {
            await _collection.InsertOneAsync(scrapRun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
        }
    }
}
