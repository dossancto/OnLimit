using OnLimit.Entities;

namespace OnLimit.Configuration;

public record PlanConfig<T>(
    string FallbackPlan,
    UsagePlanItemDict[] PlanDict,
    UsagePlanItem<T>[] Plan
    ) where T : notnull, new();

public record UsagePlanItemDict
(
    string Plan,
    IDictionary<string, long> Value
);
