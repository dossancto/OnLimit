namespace OnLimit.Postgres;

public class OnLimitPostgresConfig
{
    public string LinkTable { get; init; } = "plan_usage_link";
    public string ConsumitionTable { get; init; } = "plan_usage_consumition";
}
