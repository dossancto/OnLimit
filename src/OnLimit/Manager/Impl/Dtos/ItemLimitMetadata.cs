namespace OnLimit.Manager.Impl.Dtos;

internal class LimitItemMetadata(
        string FieldName,
        bool IsIncremental,
        bool IsUsageSwitch,
        bool IsRanged = false
        )
{
    public string FieldName { get; } = FieldName;
    public bool IsIncremental { get; } = IsIncremental;
    public bool IsRanged { get; } = IsRanged;
    public bool IsUsageSwitch { get; } = IsUsageSwitch;
    public int Count { get; init; }

    public bool Enabled { get; set; }

    public int Used { get; init; }
}
