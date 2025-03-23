using OnLimit.Entities;

namespace OnLimit.Tests.Unit.Manager.Usage;

public class MyPlan
{
    public long Users { get; set; }
}

public class SimpleUsageTests
{
    [Fact]
    public async Task SimpleUsageTests_ShouldHaveUsage_WhenAvaible()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        usageRepositoy
          .GetLatestUserPlan(Arg.Any<string>(), Arg.Any<DateTime?>())
          .Returns(new UsageUserPlans()
          {
              Plan = "FREE"
          });

        var config = new PlanConfig<MyPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
            new("FREE",
                new Dictionary<string, object>()
                {
                  ["Users"] = 10
                }
              )
            ],
            Plan: [
            new("FREE",0, new()
              {
                  Users = 10
              })
            ]
        );

        var manager = new UsageManager<MyPlan>(usageRepositoy, config);

        await manager.Usage("my org id", [
            new(x => x.Users, 5)
        ]);
    }

    [Fact]
    public async Task SimpleUsageTests_ShouldThrow_WhenResourceNotAvaible()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        usageRepositoy
          .GetLatestUserPlan(Arg.Any<string>(), Arg.Any<DateTime?>())
          .Returns(new UsageUserPlans()
          {
              Plan = "FREE"
          });

        var config = new PlanConfig<MyPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
            new("FREE",
                new Dictionary<string, object>()
                {
                  ["Users"] = 10
                }
              )
            ],
            Plan: [
            new("FREE",0, new()
              {
                  Users = 10
              })
            ]
        );

        var manager = new UsageManager<MyPlan>(usageRepositoy, config);

        var ex = await manager.Usage("my org id", [
            new(x => x.Users, 11)
        ]);

        ex.ShouldNotBeNull();

        ex.Items.ShouldHaveSingleItem();
        ex.Items.First().Field.ShouldBe("Users");
        ex.Items.First().Limit.ShouldBe(10);
        ex.Items.First().Requested.ShouldBe(11);
        ex.Items.First().IsIncremental.ShouldBe(false);
        ex.Items.First().IsUsageSwitch.ShouldBe(false);
    }

    [Fact]
    public async Task SimpleUsageTests_ShouldCreateFallbackPlan_WhenNoPlan()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        usageRepositoy
          .GetLatestUserPlan(Arg.Any<string>(), Arg.Any<DateTime?>())
          .Returns(Task.FromResult<UsageUserPlans?>(null));

        usageRepositoy
          .SetPlan(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime?>())
          .Returns(Task.CompletedTask)
          ;

        var config = new PlanConfig<MyPlan>(
            FallbackPlan: "FREE",
            PlanDict: [
            new("FREE",
                new Dictionary<string, object>()
                {
                  ["Users"] = 10
                }
              )
            ],
            Plan: [
            new("FREE",0, new()
              {
                  Users = 10
              })
            ]
        );

        var manager = new UsageManager<MyPlan>(usageRepositoy, config);

        await manager.Usage("my org id", [
            new(x => x.Users, 5)
        ]);

        await usageRepositoy.Received(1).SetPlan(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime?>());
    }

}
