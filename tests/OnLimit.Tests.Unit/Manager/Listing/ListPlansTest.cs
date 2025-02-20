namespace OnLimit.Tests.Unit.Manager.Listing;

internal class MyPlan
{
    public long Users { get; set; }
}

public class ListPlansTest
{
    [Fact]
    public void ListPlansTest_ShouldListAllPlans_WhenValidInput()
    {
        var usageRepositoy = Substitute.For<IUsageRepository>();

        var config = new PlanConfig<MyPlan>(
            FallbackPlan: "FREE",
            PlanDict: [],
            Plan: [
            new("FREE", new()
              {
                  Users = 10
              })
            ]
        );

        var manager = new UsageManager<MyPlan>(usageRepositoy, config);

        var plans = manager.ListPlans();

        plans.ShouldHaveSingleItem();

        plans[0].Plan.ShouldBe("FREE");
    }
}
