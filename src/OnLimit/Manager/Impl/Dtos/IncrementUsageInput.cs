namespace OnLimit.Manager.Impl.Dtos;

public record IncrementUsageInput(
    string Id,
    List<IncrementUsageInput.ItemDto> Items,
    DateTime? At = null
    )
{
    public record ItemDto(
        string FieldName,
        long IncrementBy
    );
}
