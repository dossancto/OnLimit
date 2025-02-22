# On Limit


## Usage

- First, define the your plans structure

> You can use many primitive types to represent your data. You can use Attributes to define custom behaviors. Example: `[IncrementalUsageLimit]` allows to increment data on a collection. 

```cs
public class MyPlan
{
    public long Users { get; set; }

    [IncrementalUsageLimit]
    public long Tokens { get; set; }

    [UsageSwitch]
    public bool CanUse { get; set; }
}
```

- Inject the service. This code will inject IUsageManager<T> using MongoDB as storage

```cs
builder.Services
    .AddOnLimit<MyPlan>(new()
    {
        Values = [
          new("FREE")
          {
            Limit = new()
            {
                Tokens = 1000,
                Users = 10,
                CanUse = false
            }
          },

          new("PREMIUM")
          {
            Limit = new()
            {
                Tokens = 5000,
                CanUse = true,
                Users = 10
            }
          }
        ]

    })
    .AddMongoDB();
```

- That it, now you can use the usage API

- Fetch Plans

Use this method to fetch you define plans. It can be used to list on some endpoint

```cs
app.MapGet("/plans", (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    return usageManager.ListPlans();
});
```

- Set Plan to user

```cs
app.MapGet("/set-plan", async (
      [FromQuery] string plan,
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    var userId = "some Id";

    await usageManager.SetPlan(userId, plan);

    return "ok";
});
```

- Then, you can use the usage API to check if the user has access to the resources. You can use many usage fields as you want on a single call. If something fails, this method will throw an Exception with details about the usages items.

```cs
app.MapGet("/consume", async (
      [FromServices] IUsageManager<MyPlan> usageManager
      ) =>
{
    var myUserId = "123";
    var actualUsers = 123;

    await usageManager.UsageAndThrow(myUserId, [
        new(x => x.Users, Count: 500, Used: actualUsers),
        new(x => x.Tokens, 500),
        new(x => x.CanUse),
    ]);

    await usageManager.Consume(myUserId, [
        new(x => x.Tokens, 500)
    ]);

    return "ok";
});
```

- After checking the usage, you can consume the resources, incrementing the usage count. Only fields with `[IncrementalUsageLimit]` can be incremented.
