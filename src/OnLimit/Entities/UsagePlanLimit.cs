namespace OnLimit.Entities;

public record UsagePlanItem<T>(
    string Plan,
    T Limit
  ) where T : notnull, new()
{
    public UsagePlanItem() : this("", new T()) { }
    public UsagePlanItem(string plan) : this(plan, new T()) { }
}
