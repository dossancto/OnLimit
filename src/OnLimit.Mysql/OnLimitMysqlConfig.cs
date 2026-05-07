namespace OnLimit.Mysql;

public class OnLimitMysqlConfig
{
    public string LinkTable { get; init; } = "plan_usage_link";

    public string ConsumitionTable { get; init; } = "plan_usage_consumition";

    public string LimitsTable { get; init; } = "plan_usage_limits";
}