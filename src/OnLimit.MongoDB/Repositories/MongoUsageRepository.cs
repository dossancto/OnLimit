using MongoDB.Bson;
using MongoDB.Driver;
using OnLimit.Entities;
using OnLimit.Interfaces;

namespace OnLimit.MongoDB.Repositories;

public class MongoUsageRepository(
    OnLimitMongoDBConfiguration config,
    IMongoClient mongo
    ) : IUsageRepository
{
    private readonly OnLimitMongoDBConfiguration config = config;
    private readonly IMongoClient mongo = mongo;

    private IMongoCollection<UsageUserPlans> LinkCollection
        = mongo
        .GetDatabase(config.Database)
        .GetCollection<UsageUserPlans>(config.LinkCollection);

    public Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at)
    {
        throw new NotImplementedException();
    }

    public async Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null)
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
