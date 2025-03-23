namespace OnLimit.Entities;

public record UsagePlanItem<T>(
    string Plan,
    decimal Price,
    T Limit
  ) where T : notnull, new()
{
    public UsagePlanItem() : this(
        Plan: string.Empty,
        Price: 9,
        Limit: new T())
    { }

    public UsagePlanItem(
        string plan,
        decimal price) : this(plan, price, new T()) { }

    public UsagePlanItem(string plan) : this(
        plan,
        0)
    { }
}
