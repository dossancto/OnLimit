using System.Linq.Expressions;
using System.Reflection;
using OnLimit.Configuration;
using OnLimit.Entities;
using OnLimit.Interfaces;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit;

public class UsageManager<T>(
      IUsageRepository usageRepository,
      PlanConfig<T> config
    ) : IUsageManager<T> where T : notnull, new()
{
    private readonly IUsageRepository usageRepository = usageRepository;
    private readonly PlanConfig<T> config = config;

    public Task<UsageUserPlans?> GetActualPlan(string Id, DateTime? at = null)
    => usageRepository.GetLatestUserPlan(Id, at);

    public UsagePlanItem<T>[] ListPlans()
    => config.Plan;

    public async Task SetPlan(string orgId, string plan, DateTime? at = null)
    {
        var validPlan = config.PlanDict.FirstOrDefault(x => x.Plan == plan) is not null;

        if (validPlan is false)
        {
            throw new Exception($"Invalid Plan: {plan}");
        }

        await usageRepository.SetPlan(orgId, plan, at);
    }

    private Exception? Process(
        DateTime? at,
        LimitItemMetadata order,
        IDictionary<string, long> plan,
        long? consumition
        )
    {
        var requetedCount = order.Count;

        var requiredAmmount = requetedCount + (consumition ?? order.Used);

        var field = order.FieldName;
        var planLimit = plan[field];

        if (planLimit < requiredAmmount)
        {
            return new($"Invalid Usage for {field} on plan . Requested: {requiredAmmount}, Limit: {planLimit}");
        }

        return null;
    }

    public async Task Usage(string Id, CheckPlanUsageInput<T>[] exprs, DateTime? at = null)
    {
        var now = at ?? DateTime.UtcNow;

        Dictionary<string, long>? consumition = null;

        var actualPlan = await GetActualPlan(Id, now);

        string targetPlan;

        if (actualPlan is not null)
        {
            targetPlan = actualPlan.Plan;
        }
        else
        {
            targetPlan = config.FallbackPlan;

            await SetPlan(Id, config.FallbackPlan);
        }

        var plan = config.PlanDict.FirstOrDefault(x => x.Plan == targetPlan)?.Value;

        if (plan is null)
        {
            throw new ArgumentException($"Plan {targetPlan} not found");
        }

        var limitFields = exprs
          .Select(GetFieldName)
          .ToList();

        var incrementalLimitFields = limitFields
          .Where(x => x.IsIncremental is true)
          .ToList();

        if (incrementalLimitFields.Count is not 0)
        {
            consumition = await usageRepository.GetConsumition(Id, now);
        }

        var res = limitFields.Select(x => Process(
            at: at,
            order: x,
            plan: plan,
            consumition: consumition is null
            ? null
            : (consumition.TryGetValue(x.FieldName, out var value) ? value : null)
        ));

        var failedItems = res
                        .Where(x => x is not null)
                        .Cast<Exception>()
                        .ToArray();

        if (failedItems.Any())
        {
            throw new AggregateException(failedItems);
        }
    }

    private (MemberExpression Member, PropertyInfo Property) GetMemberExpression(Expression<Func<T, object>> expression)
    {
        var member = expression.Body as MemberExpression ??
                     (expression.Body as UnaryExpression)?.Operand as MemberExpression;

        if (member is null)
        {
            throw new ArgumentException("The lambda expression 'propertyLambda' should point to a valid Property");
        }

        var propertyInfo = member.Member as PropertyInfo;

        if (propertyInfo is null)
        {
            throw new ArgumentException("The lambda expression 'propertyLambda' should point to a valid Property");
        }

        return (member, propertyInfo);
    }

    private LimitItemMetadata GetFieldName(CheckPlanUsageInput<T> item)
    {
        var (member, propertyInfo) = GetMemberExpression(item.expr);

        var fieldName = member.Member.Name;

        var incrementalUsage = propertyInfo.GetCustomAttribute<IncrementalUsageLimitAttribute>();

        return new(
                fieldName,
                incrementalUsage is not null
            )
        {
            Count = item.Count,
            Used = item.Used
        };
    }

    public Task Consume(ConsumeUsageInput<T> input)
      => usageRepository.Increment(new(
            Id: input.UserId,

            Items: input.Items.Select(x => new IncrementUsageInput.ItemDto(
                GetMemberExpression(x.expr).Member.Member.Name,
                x.IncrementBy
                )).ToList(),

            At: input.At
            ));
}
