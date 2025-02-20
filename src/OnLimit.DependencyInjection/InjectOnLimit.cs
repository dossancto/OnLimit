using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OnLimit.Configuration;
using OnLimit.Entities;
using OnLimit.Interfaces;

namespace OnLimit.DependencyInjection;

public static class InjectOnLimit
{
    public static OnLimitServicesBuilder AddOnLimit<T>(
        this IServiceCollection services,
        OnLimitServiceConfiguration<T> config
        ) where T : notnull, new()
    {
        var FallbackPlan = config.FallbackPlan ?? config.Values.First().Plan;

        var itemsAsDict = config.Values
            .Select(x => new UsagePlanItemDict(x.Plan, x.Value.ToDictionary()))
            .ToArray();

        var c = new PlanConfig<T>(FallbackPlan, itemsAsDict, config.Values);

        services.AddSingleton(c);

        services.AddScoped<IUsageManager<T>, UsageManager<T>>();

        return new(services);
    }

    private static IDictionary<string, long> ToDictionary(this object source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var dictionary = new Dictionary<string, long>();
        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(source);

            var a = value as long? ?? 0;
            dictionary.Add(property.Name, a);
        }

        return dictionary;
    }
}

public record OnLimitServiceConfiguration<T>(
    UsagePlanItem<T>[] Values,
    string? FallbackPlan
    ) where T : notnull, new()
{
    public OnLimitServiceConfiguration() : this(
        Values: [],
        FallbackPlan: null
        )
    { }

}

public record OnLimitServicesBuilder(IServiceCollection services)
{

}
