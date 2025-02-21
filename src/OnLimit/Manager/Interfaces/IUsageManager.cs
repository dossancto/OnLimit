using OnLimit.Entities;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit.Interfaces;

public interface IUsageManager<T> where T : notnull, new()
{
    Task Usage(string Id, CheckPlanUsageInput<T>[] exprs, DateTime? at = null);

    Task<UsageUserPlans?> GetActualPlan(string Id, DateTime? at = null);

    Task SetPlan(string orgId, string plan, DateTime? at = null);

    Task Consume(string UserId, List<ConsumeUsageInput<T>> Items, DateTime? At = null);

    UsagePlanItem<T>[] ListPlans();
}
