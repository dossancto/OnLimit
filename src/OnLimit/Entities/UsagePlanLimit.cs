namespace OnLimit.Entities;

public record UsagePlanItem<T>(
    string Plan,
    T Value
  ) where T : notnull, new()
{
    public UsagePlanItem() : this("", new T()) { }
}
