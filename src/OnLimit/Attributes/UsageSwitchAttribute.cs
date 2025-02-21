namespace OnLimit;

[AttributeUsage(AttributeTargets.Property)]
public class UsageSwitchAttribute() : Attribute
{
    public bool IsUsageSwitch { get; } = true;
}

