namespace OnLimit;

[AttributeUsage(AttributeTargets.Property)]
public class IncrementalUsageLimitAttribute() : Attribute
{
    public bool IsIncremental { get; } = true;
}
