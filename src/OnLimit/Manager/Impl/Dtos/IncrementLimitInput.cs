using System.Linq.Expressions;

namespace OnLimit.Manager.Impl.Dtos;

public record IncrementLimitInput<T>
(
      Expression<Func<T, object>> expr, long IncrementBy
);

public record IncrementLimitRequest
(
    string Id,
    List<IncrementLimitRequest.ItemDto> Items,
    DateTime? At = null
)
{
    public record ItemDto(
        string FieldName,
        long IncrementBy
    );
}
