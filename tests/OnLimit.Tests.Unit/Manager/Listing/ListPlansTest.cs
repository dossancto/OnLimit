namespace OnLimit.Tests.Unit.Manager.Listing;

public class MyPlan
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
            new("FREE", 15, new()
              {
                  Users = 10
              }),
            new("FREE2", 15)
            {
              Limit = new()
                {
                  Users = 16
                }
              },
            ]
        );

        var manager = new UsageManager<MyPlan>(usageRepositoy, config);

        var plans = manager.ListPlans();

        plans.Count().ShouldBe(2);

        plans[0].Plan.ShouldBe("FREE");
        plans[0].Price.ShouldBe(15);
    }
}
