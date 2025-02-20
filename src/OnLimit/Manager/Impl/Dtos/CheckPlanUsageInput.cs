using System.Linq.Expressions;

namespace OnLimit.Manager.Impl.Dtos;

public record CheckPlanUsageInput<T>
(
      Expression<Func<T, object>> expr,
      int Count,
      int Used = 0
);
