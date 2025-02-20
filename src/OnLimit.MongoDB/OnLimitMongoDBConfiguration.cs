namespace OnLimit.MongoDB;

public class OnLimitMongoDBConfiguration
{
    public string Database { get; init; } = "plan_usage";
    public string LinkCollection { get; init; } = "plan_usage_link";
    public string ConsumitionCollection { get; init; } = "plan_usage_link";
}
