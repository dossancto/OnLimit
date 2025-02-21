using System.Linq.Expressions;

namespace OnLimit.Manager.Impl.Dtos;

public record ConsumeUsageInput<T>(
        Expression<Func<T, object>> expr,
        long IncrementBy
    )
{
}
