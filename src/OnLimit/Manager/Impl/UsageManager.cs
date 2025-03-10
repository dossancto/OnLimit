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
    => usageRepository.GetCurrentPlan(Id, at);

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

    private OutOfUsageException.OutOfUsageItem? Process(
        DateTime? at,
        LimitItemMetadata order,
        IDictionary<string, object> plan,
        long? consumition,
        string planName
        )
    {

        var requetedCount = order.Count;

        var used = consumition ?? order.Used;
        var requiredAmmount = requetedCount + used;

        var field = order.FieldName;
        var planLimit = plan[field];

        if (planLimit is long planLimitLong)
        {
            if (planLimitLong < requiredAmmount)
            {
                return new(
                    Plan: planName,
                    Field: field
                    )
                {
                    Requested = requiredAmmount,
                    Limit = planLimitLong,
                    Used = used
                };
            }
        }

        else if (planLimit is decimal planLimitDecimal)
        {
            if (planLimitDecimal < requiredAmmount)
            {
                return new(
                    Plan: planName,
                    Field: field
                    )
                {
                    Requested = requiredAmmount,
                    Limit = planLimitDecimal,
                    IsIncremental = order.IsIncremental,
                    Used = used
                };
            }
        }
        else if (planLimit is double planLimitDouble)
        {
            if (planLimitDouble < requiredAmmount)
            {
                return new(
                    Plan: planName,
                    Field: field
                    )
                {
                    Requested = requiredAmmount,
                    Limit = planLimitDouble,
                    IsIncremental = order.IsIncremental,
                    Used = used
                };
            }
        }
        else if (planLimit is int planLimitInt)
        {
            if (planLimitInt < requiredAmmount)
            {
                return new(
                    Plan: planName,
                    Field: field
                    )
                {
                    Requested = requiredAmmount,
                    Limit = planLimitInt,
                    IsIncremental = order.IsIncremental,
                    Used = used
                };
            }
        }
        else if (planLimit is bool planLimitBool)
        {
            if (planLimitBool is false)
            {
                return new(
                    Plan: planName,
                    Field: field
                    )
                {
                    Requested = requiredAmmount,
                    IsUsageSwitch = true
                };
            }
        }
        else
        {
            throw new ArgumentException($"Type not supported {planLimit.GetType()}");
        }


        return null;
    }

    public async Task<OutOfUsageException?> Usage(string Id, CheckPlanUsageInput<T>[] exprs, DateTime? at = null)
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
            var lastPlan = await usageRepository.GetLatestUserPlan(Id);

            targetPlan = lastPlan?.Plan ?? config.FallbackPlan;

            await SetPlan(Id, targetPlan);
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
            consumition: consumition?.GetValueOrDefault(x.FieldName),
            planName: targetPlan
        ));

        var failedItems = res
                        .Where(x => x is not null)
                        .ToArray();

        if (failedItems.Any())
        {
            return new OutOfUsageException(failedItems!);
        }

        return null;
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
        var usageSwitch = propertyInfo.GetCustomAttribute<UsageSwitchAttribute>();

        return new(
                FieldName: fieldName,
                IsIncremental: incrementalUsage is not null,
                IsUsageSwitch: usageSwitch is not null
            )
        {
            Count = item.Count,
            Enabled = item.Enabled,
            Used = item.Used
        };
    }

    public async Task Consume(string UserId, List<ConsumeUsageInput<T>> Items, DateTime? At = null)
    {
        var items = new IncrementUsageInput(
               Id: UserId,

               Items: Items
               .Select(x =>
               {
                   var (member, propertyInfo) = GetMemberExpression(x.expr);

                   var incrementalUsage = propertyInfo.GetCustomAttribute<IncrementalUsageLimitAttribute>() is not null;

                   return (x.IncrementBy, member, incrementalUsage);
               })
               .Where(x => x.incrementalUsage is true)
               .Select(x => new IncrementUsageInput.ItemDto(
                 x.member.Member.Name,
                 x.IncrementBy
                 )).ToList(),

               At: At
         );

        if (items.Items.Count is 0)
        {
            return;
        }

        await usageRepository.Increment(items);
    }

    public async Task UsageAndThrow(string Id, CheckPlanUsageInput<T>[] exprs, DateTime? at = null)
    {
        var usage = await Usage(Id, exprs, at);

        if (usage is not null)
        {
            throw usage;
        }
    }

    public Task<Dictionary<string, long>> GetConsumition(string Id, DateTime? at = null)
    => usageRepository.GetConsumition(Id, at ?? DateTime.UtcNow);
}
