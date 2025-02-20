namespace OnLimit.Manager.Impl.Dtos;

internal class LimitItemMetadata(
        string FieldName,
        bool IsIncremental
        )
{
    public string FieldName { get; } = FieldName;
    public bool IsIncremental { get; } = IsIncremental;
    public int Count { get; init; }

    public int Used { get; init; }
}
