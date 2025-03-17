using OnLimit.Entities;
using OnLimit.FieldConfigs;
using OnLimit.Manager.Impl.Dtos;

namespace OnLimit.Tests.Unit.Manager.FieldConfigs;

public class RangedConfigPlan
{
    [IncrementalUsageLimit]
    public RangedField Tokens { get; set; } = new();
}

public class RangedFieldConfigTest
{
    [Fact]
    public async Task TestSomeThingLaAsync()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        usageRepositoy
          .GetLatestUserPlan(Arg.Any<string>())
          .Returns(new UsageUserPlans()
          {
              Plan = "FREE"
          });

        usageRepositoy
          .GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>())
          .Returns(
                  new Dictionary<string, long>()
                  {
                      ["Tokens"] = 1
                  }
              );

        var config = new PlanConfig<RangedConfigPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
              new("FREE",
                new Dictionary<string, object>()
                  {
                    ["Tokens"] = new RangedField(10, 1000)
                  }
                )
            ],
            Plan: [
              new("FREE", new()
                {
                    Tokens = new(10, 1000)
                })
            ]
        );

        var manager = new UsageManager<RangedConfigPlan>(usageRepositoy, config);

        var res = await manager.Usage("my org id", [
            new(expr: x => x.Tokens, Count: 5)
        ]);

        res.ShouldBeNull();

        await manager.Consume("my org id", [
            new(x => x.Tokens, 100)
        ]);

        await usageRepositoy.Received(1).GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>());

        await usageRepositoy.Received(1).Increment(Arg.Is<IncrementUsageInput>(x => x.Items.Any(y => y.FieldName == "Tokens")));
    }

    [Fact]
    public async Task IncrementalFieldConfigTest_ShouldReturnFail_WhenOutOfUsage()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        usageRepositoy
          .GetLatestUserPlan(Arg.Any<string>())
          .Returns(new UsageUserPlans()
          {
              Plan = "FREE"
          });

        usageRepositoy
          .GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>())
          .Returns(
                  new Dictionary<string, long>()
                  {
                      ["Tokens"] = 999
                  }
              );

        var config = new PlanConfig<RangedConfigPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
              new("FREE",
                new Dictionary<string, object>()
                  {
                    ["Tokens"] = new RangedField(10, 1000)
                  }
                )
            ],
            Plan: [
              new("FREE", new()
                {
                    Tokens = new(10, 1000)
                })
            ]
        );

        var manager = new UsageManager<RangedConfigPlan>(usageRepositoy, config);

        var res = await manager.Usage("my org id", [
            new(expr: x => x.Tokens, Count: 5)
        ]);

        res.ShouldNotBeNull();

        await manager.Consume("my org id", [
            new(x => x.Tokens, 100)
        ]);

        await usageRepositoy.Received(1).GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>());

        await usageRepositoy.Received(1).Increment(Arg.Is<IncrementUsageInput>(x => x.Items.Any(y => y.FieldName == "Tokens")));
    }
}
