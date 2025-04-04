using OnLimit.Entities;
using OnLimit.FieldConfigs;

namespace OnLimit.Tests.Unit.Manager.Usage;

public class IncrementalPlan
{
    public RangedField Tokens { get; set; } = new();
}

public class IncrementalUsageLimitsTests
{
    [Fact]
    public async Task IncrementalUsageLimitsTests_ShouldUseStoredUsage_WhenIncrementalField()
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

        var config = new PlanConfig<IncrementalPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
              new("FREE",
                new Dictionary<string, object>()
                  {
                    ["Tokens"] = new RangedField(0, 10)
                  }
                )
            ],
            Plan: [
              new("FREE",0, new()
                {
                    Tokens = new(0, 10)
                })
            ]
        );

        var manager = new UsageManager<IncrementalPlan>(usageRepositoy, config);

        await manager.Usage("my org id", [
            new(x => x.Tokens, 5)
        ]);

        await usageRepositoy.Received(1).GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task IncrementalUsageLimitsTests_ShouldUseStoredUsageAndThrow_WhenIncrementalField()
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
                      ["Tokens"] = 9
                  }
              );

        var config = new PlanConfig<IncrementalPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
              new("FREE",
                new Dictionary<string, object>()
                  {
                    ["Tokens"] = new RangedField(0, 10)
                  }
                )
            ],
            Plan: [
              new("FREE",0, new()
                {
                    Tokens = new(0, 10)
                })
            ]
        );

        var manager = new UsageManager<IncrementalPlan>(usageRepositoy, config);

        var ex = await manager.Usage("my org id", [
            new(x => x.Tokens, 5)
        ]);

        ex.ShouldNotBeNull();

        await usageRepositoy.Received(1).GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>());

        ex.Items.ShouldHaveSingleItem();
        ex.Items.First().Field.ShouldBe(nameof(IncrementalPlan.Tokens));
        ex.Items.First().Limit.ShouldBe(10);
        ex.Items.First().Requested.ShouldBe(14);
        ex.Items.First().IsRanged.ShouldBe(true);
        ex.Items.First().IsUsageSwitch.ShouldBe(false);
    }

    [Fact]
    public async Task IncrementalUsageLimitsTests_ShouldContinue_WhenNoConsumeCreated()
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
                  }
              );

        var config = new PlanConfig<IncrementalPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
              new("FREE",
                new Dictionary<string, object>()
                  {
                    ["Tokens"] = new RangedField(0, 10)
                  }
                )
            ],
            Plan: [
              new("FREE",0, new()
                {
                    Tokens = new(0, 10)
                })
            ]
        );

        var manager = new UsageManager<IncrementalPlan>(usageRepositoy, config);

        await manager.Usage("my org id", [
                    new(x => x.Tokens, 5)
                ]);

        await usageRepositoy.Received(1).GetConsumition(Arg.Any<string>(), Arg.Any<DateTime>());
    }

}
