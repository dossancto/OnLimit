using OnLimit.Entities;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit.Interfaces;

public interface IUsageRepository
{
    /// <summarry>
    /// Set a plan to User on a Configuration Table
    /// </summarry>
    /// <param name="orgId">User id as key</param>
    /// <param name="plan">The plan name to be set</param>
    /// <param name="at">The moment the plan was set. Null for now</param>
    Task SetPlan(string orgId, string plan, DateTime? at = null);

    /// <summarry>
    /// Get the latest plan set on a Configuration Table
    /// </summarry>
    /// <param name="Id">User id as key</param>
    Task<UsageUserPlans?> GetLatestUserPlan(string Id);

    /// <summarry>
    /// Get the latest plan set on a Configuration Table
    /// </summarry>
    /// <param name="Id">User id as key</param>
    [Obsolete("Use GetLatestUserPlan instead")]
    Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null);

    /// <summarry>
    /// Get the current plan set on a Configuration Table.
    /// </summarry>
    /// <param name="Id">User id as key</param>
    Task<UsageUserPlans?> GetCurrentPlan(string Id, DateTime? at = null);

    /// <summarry>
    /// Get the plan Items consumition
    /// </summarry>
    /// <param name="orgId">User id as key</param>
    /// <param name="at">The moment the plan was set</param>
    Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at);

    /// <summarry>
    /// Get Increased Limits on a Configuration Table
    /// </summarry>
    /// <param name="orgId">User id as key</param>
    Task<Dictionary<string, long>> GetLimits(string orgId);

    /// <summarry>
    /// Increment the usage of some items
    /// </summarry>
    /// <param name="input">Data used to increment the plans</param>
    Task Increment(IncrementUsageInput input);

    /// <summarry>
    /// Increment the usage limit from some user. This only works for `IncrementField` fields.
    /// </summarry>
    /// <param name="input">Data used to increment the field</param>
    Task IncrementLimit(IncrementLimitRequest input);
}

