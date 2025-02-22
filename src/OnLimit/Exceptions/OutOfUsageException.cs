namespace OnLimit;

public class OutOfUsageException(params OutOfUsageException.OutOfUsageItem[] items) : Exception
{
    public OutOfUsageItem[] Items { get; } = items;

    public record OutOfUsageItem
      (
        string Plan,
        string Field,
        long Requested = 0,
        long Limit = 0,
        long Used = 0,
        bool IsIncremental = false,
        bool IsUsageSwitch = false
      );
}
