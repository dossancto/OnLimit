using MongoDB.Bson;
using MongoDB.Driver;
using OnLimit.Entities;
using OnLimit.Interfaces;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit.MongoDB.Repositories;

public class MongoUsageRepository(
    OnLimitMongoDBConfiguration config,
    IMongoClient mongo
    ) : IUsageRepository
{
    private readonly OnLimitMongoDBConfiguration config = config;
    private readonly IMongoClient mongo = mongo;

    private IMongoCollection<BsonDocument> ConsumitionCollection
        = mongo
        .GetDatabase(config.Database)
        .GetCollection<BsonDocument>(config.ConsumitionCollection);

    private IMongoCollection<UsageUserPlans> LinkCollection
        = mongo
        .GetDatabase(config.Database)
        .GetCollection<UsageUserPlans>(config.LinkCollection);

    public async Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at)
    {
        var f = Builders<BsonDocument>.Filter;

        var date = UsageUserPlans.MapDate(at);

        var projection = Builders<BsonDocument>.Projection
          .Exclude("_id")
          .Exclude("Date")
          .Exclude("UserId")
          ;

        var filter = f
        .Eq(nameof(UsageUserPlans.UserId), orgId)
            & f.Eq(nameof(UsageUserPlans.Date), date);
        ;

        var result = await ConsumitionCollection
        .Find(filter)
        .Project(projection)
        .FirstOrDefaultAsync()
        ;

        return result?.ToDictionary(x => x.Name, x =>
            long.TryParse(x.Value.ToString(), out var i) ? i : 0
            ) ?? new();
    }

    public async Task<UsageUserPlans?> GetCurrentPlan(string Id, DateTime? at = null)
    {
        var now = at ?? DateTime.Now;

        var f = Builders<UsageUserPlans>.Filter;

        var date = UsageUserPlans.MapDate(now);

        var filter = f
        .Eq(x => x.UserId, Id)
            & f.Eq(x => x.Date, date);
        ;

        var result = await LinkCollection
        .Find(filter)
        .SortByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync()
        ;

        return result;
    }

    public async Task<UsageUserPlans?> GetLatestUserPlan(string Id)
    {
        var f = Builders<UsageUserPlans>.Filter;

        var filter = f.Eq(x => x.UserId, Id);

        var result = await LinkCollection
        .Find(filter)
        .SortByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync()
        ;

        return result;
    }

    public Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, long>> GetLimits(string orgId)
    {
        throw new NotImplementedException();
    }

    public async Task Increment(IncrementUsageInput input)
    {
        var now = input.At ?? DateTime.UtcNow;

        var f = Builders<BsonDocument>.Filter;

        var date = UsageUserPlans.MapDate(now);

        var filter = f
        .Eq(nameof(UsageUserPlans.UserId), input.Id)
            & f.Eq(nameof(UsageUserPlans.Date), date);
        ;

        var u = Builders<BsonDocument>.Update;

        var update = u.Combine(
                input.Items.Select(x => u.Inc(x.FieldName, x.IncrementBy))
            );

        var options = new UpdateOptions { IsUpsert = true };

        var result = await ConsumitionCollection
          .UpdateManyAsync(filter, update, options);
    }

    public Task IncrementLimit(IncrementLimitRequest input)
    {
        throw new NotImplementedException();
    }

    public async Task SetPlan(string orgId, string plan, DateTime? at = null)
    {
        var now = at ?? DateTime.Now;

        var model = new UsageUserPlans()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = orgId,
            CreatedAt = now,
            Plan = plan,
            Date = UsageUserPlans.MapDate(now),
        };

        await LinkCollection.InsertOneAsync(model);
    }
}
