using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using WebScrapper.ScraperApi.Entities;
using WebScrapper.ScraperApi.Repositories.Interfaces;

namespace WebScrapper.ScraperApi.Repositories;

public class ScrapJobsRepository : IScrapJobsRepository
{
    private readonly ILogger _logger;

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScrapJob> _collection;

    public ScrapJobsRepository(ILogger<ScrapJobsRepository> logger, IMongoDatabase database)
    {
        _logger = logger;
        _database = database;
        _collection = _database.GetCollection<ScrapJob>("ScrapJobs");
    }

    public async Task<List<ScrapJob>> GetAllAsync(bool? active = null)
    {
        try
        {
            var pipeline = new List<BsonDocument>();

            if (active.HasValue)
            {
                pipeline.Add(new BsonDocument("$match", new BsonDocument("IsActive", active.Value)));
            }

            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "WebsiteMetadatas" },
                { "localField", "WebsiteMetadataId" },
                { "foreignField", "_id" },
                { "as", "_website" }
            }));
            pipeline.Add(new BsonDocument("$addFields", new BsonDocument(
                "WebsiteName",
                new BsonDocument("$arrayElemAt", new BsonArray { "$_website.Name", 0 }))));
            pipeline.Add(new BsonDocument("$project", new BsonDocument("_website", 0)));
            pipeline.Add(new BsonDocument("$sort", new BsonDocument("CreatedOn", -1)));

            return await _collection.Aggregate<ScrapJob>(pipeline).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            return [];
        }
    }

    public async Task<ScrapJob> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<ScrapJob>.Filter.Eq(x => x.Id, id);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            if (result is null)
                throw new InvalidOperationException($"ScrapJob with ID {id} not found in collection.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task AddAsync(ScrapJob scrapJob)
    {
        try
        {
            scrapJob.WebsiteName = null;
            if (scrapJob.CreatedOn == default)
                scrapJob.CreatedOn = DateTime.UtcNow;
            await _collection.InsertOneAsync(scrapJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
        }
    }

    public async Task UpdateAsync(string id, ScrapJob scrapJob)
    {
        try
        {
            scrapJob.Id = id;
            scrapJob.WebsiteName = null;
            var filter = Builders<ScrapJob>.Filter.Eq(x => x.Id, id);
            await _collection.ReplaceOneAsync(filter, scrapJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task PatchAsync(string id, JsonElement updates)
    {
        try
        {
            var raw = BsonDocument.Parse(updates.GetRawText());

            // Entity is serialized to BSON with PascalCase field names (no camelCase
            // convention is registered). FE forms send camelCase JSON, so we have to
            // PascalCase the top-level keys before $set, otherwise the update writes
            // sibling camelCase fields that the entity ignores on read.
            var set = new BsonDocument();
            foreach (var element in raw.Elements)
            {
                var name = element.Name;
                if (name is "_id" or "Id" or "id" or "WebsiteName" or "websiteName" or "CreatedOn" or "createdOn")
                    continue;

                var pascalName = name.Length > 0 ? char.ToUpperInvariant(name[0]) + name[1..] : name;
                set[pascalName] = element.Value;
            }

            if (set.ElementCount == 0) return;

            var filter = Builders<ScrapJob>.Filter.Eq(x => x.Id, id);
            await _collection.UpdateOneAsync(filter, new BsonDocument("$set", set));
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
            var filter = Builders<ScrapJob>.Filter.Eq(x => x.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB operation failed: {Message}", ex.Message);
            throw;
        }
    }
}
