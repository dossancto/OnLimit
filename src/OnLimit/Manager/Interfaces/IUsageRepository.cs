using OnLimit.Entities;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit.Interfaces;

public interface IUsageRepository
{
    Task SetPlan(string orgId, string plan, DateTime? at = null);

    Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null);

    Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at);

    Task Increment(IncrementUsageInput input);
}

