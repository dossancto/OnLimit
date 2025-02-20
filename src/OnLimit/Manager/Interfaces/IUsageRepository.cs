using OnLimit.Entities;

namespace OnLimit.Interfaces;

public interface IUsageRepository
{
    Task SetPlan(string orgId, string plan, DateTime? at = null);

    Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null);

    Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at);
}

