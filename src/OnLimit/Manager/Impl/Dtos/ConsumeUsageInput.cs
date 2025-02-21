using System.Linq.Expressions;

namespace OnLimit.Manager.Impl.Dtos;

public record ConsumeUsageInput<T>(
    string UserId,
    List<ConsumeUsageInput<T>.ItemDto> Items,
    DateTime? At = null
    )
{
    public record ItemDto(
        Expression<Func<T, object>> expr,
        long IncrementBy
    );
}
