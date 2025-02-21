using System.Linq.Expressions;

namespace OnLimit.Manager.Impl.Dtos;

public record CheckPlanUsageInput<T>
(
      Expression<Func<T, object>> expr,
      int Count = 0,
      int Used = 0,
      bool Enabled = false
)
{
    public CheckPlanUsageInput(Expression<Func<T, object>> expr) : this(expr, 0, 0)
    {

    }
}
