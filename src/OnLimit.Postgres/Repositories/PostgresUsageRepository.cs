using Dapper;
using Npgsql;
using OnLimit.Entities;
using OnLimit.Interfaces;
using OnLimit.Manager.Impl.Dtos;
using OnLimit.Postgres.Entities;

namespace OnLimit.Postgres.Repositories;

public class PostgresUsageRepository(
    NpgsqlConnection connection,
    OnLimitPostgresConfig config
    ) : IUsageRepository
{
    private readonly NpgsqlConnection connection = connection;
    private readonly OnLimitPostgresConfig config = config;

    public async Task<Dictionary<string, long>> GetConsumition(string orgId, DateTime at)
    {
        var date = UsageUserPlans.MapDate(at);
        var SQL = $@"SELECT * FROM ""{config.ConsumitionTable}"" WHERE ""UserId"" = @id AND ""Date"" = @date;";

        var values = new
        {
            id = orgId,
            date = date
        };

        var result = await connection.QueryFirstOrDefaultAsync(SQL, values);

        if (result is null) return new();

        Dictionary<string, object> res = DynamicToDictionary(result);

        var b = res
          .Where(x => !(x.Key is "UserId" or "Id"))
          .Select(x =>
        {
            var (key, val) = x;

            if (long.TryParse(val.ToString(), out long bla))
            {
                return new KeyValuePair<string, long>(key, bla);
            }

            return new();
        })
        .Where(x => string.IsNullOrWhiteSpace(x.Key) is false)
        ;

        return b.ToDictionary();
    }

    public async Task<UsageUserPlans?> GetCurrentPlan(string Id, DateTime? at = null)
    {
        var now = at ?? DateTime.Now;
        var date = UsageUserPlans.MapDate(now);

        var SQL = $"SELECT * FROM \"{config.LinkTable}\" WHERE \"UserId\" = @id AND \"Date\" = @date;";

        var res = await connection.QueryFirstOrDefaultAsync<PostgresUsageUserPlans>(SQL, new
        {
            id = Guid.Parse(Id),
            date = date
        });

        return res?.MapToDomain();
    }

    public async Task<UsageUserPlans?> GetLatestUserPlan(string Id)
    {
        var query = $"SELECT * FROM \"{config.LinkTable}\" WHERE \"UserId\" = @id ORDER BY \"CreatedAt\" DESC LIMIT 1;";

        var res = await connection.QueryFirstOrDefaultAsync<PostgresUsageUserPlans>(query, new { id = Id });

        return res?.MapToDomain();
    }

    public Task<UsageUserPlans?> GetLatestUserPlan(string Id, DateTime? at = null)
    => GetCurrentPlan(Id);

    public async Task Increment(IncrementUsageInput input)
    {
        // using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var item in input.Items)
            {
                var selectquery = $"SELECT * FROM \"{config.ConsumitionTable}\" WHERE \"UserId\" = @id AND \"Date\" = @date;";

                var exist = await connection.QueryFirstOrDefaultAsync(selectquery, new
                {
                    id = input.Id,
                    date = UsageUserPlans.MapDate(input.At ?? DateTime.UtcNow)
                });

                if (exist is null)
                {
                    var insertquery = $"INSERT INTO \"{config.ConsumitionTable}\" (\"UserId\", \"Date\", \"{item.FieldName}\") VALUES (@id, @date, @val);";
                    await connection.ExecuteAsync(insertquery, new
                    {
                        id = input.Id,
                        date = UsageUserPlans.MapDate(input.At ?? DateTime.UtcNow),
                        val = item.IncrementBy
                    });
                    continue;
                }

                var SQL = @$"UPDATE ""{config.ConsumitionTable}""
                  SET ""{item.FieldName}"" = ""{item.FieldName}"" + @increment
                  WHERE ""UserId"" = @id;";

                await connection.ExecuteAsync(SQL, new
                {
                    id = input.Id,
                    increment = item.IncrementBy
                });
            }

            // await transaction.CommitAsync();
        }
        catch
        {
            // await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SetPlan(string orgId, string plan, DateTime? at = null, string? externalPaymentId = null)
    {
        var now = at ?? DateTime.Now;

        var model = new PostgresUsageUserPlans()
        {
            Id = Guid.NewGuid(),
            UserId = orgId,
            CreatedAt = now,
            Plan = plan,
            ExternalPaymentId = externalPaymentId,
            Date = UsageUserPlans.MapDate(now),
        };

        var SQL = $@"INSERT INTO ""{config.LinkTable}""
          (""Id"", ""UserId"", ""Plan"", ""Date"", ""CreatedAt"", ""ExternalPaymentId"")
          VALUES (@Id, @UserId, @Plan, @Date, @CreatedAt, @ExternalPaymentId);
        ";

        await connection.ExecuteAsync(SQL, model);
    }

    private static Dictionary<string, object> DynamicToDictionary(dynamic obj)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var property in (IDictionary<string, object>)obj)
        {
            dictionary.Add(property.Key, property.Value);
        }
        return dictionary;
    }

    public async Task<Dictionary<string, long>> GetLimits(string orgId)
    {
        var SQL = $@"SELECT * FROM ""{config.LimitsTable}"" WHERE ""UserId"" = @userId";

        var values = new
        {
            userId = orgId
        };

        var result = await connection.QueryFirstOrDefaultAsync(SQL, values);

        if (result is null) return new();

        Dictionary<string, object> res = DynamicToDictionary(result);

        var b = res
          .Where(x => !(x.Key is "UserId" or "Id"))
          .Select(x =>
        {
            var (key, val) = x;

            if (long.TryParse(val.ToString(), out long bla))
            {
                return new KeyValuePair<string, long>(key, bla);
            }

            return new();
        })
        .Where(x => string.IsNullOrWhiteSpace(x.Key) is false)
        ;

        return b.ToDictionary();
    }

    public async Task IncrementLimit(IncrementLimitRequest input)
    {
        // using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var item in input.Items)
            {
                var selectquery = $"SELECT * FROM \"{config.LimitsTable}\" WHERE \"UserId\" = @id";

                var exist = await connection.QueryFirstOrDefaultAsync(selectquery, new
                {
                    id = input.Id,
                });

                if (exist is null)
                {
                    var insertquery = $"INSERT INTO \"{config.LimitsTable}\" (\"UserId\", \"{item.FieldName}\") VALUES (@id, @val);";
                    await connection.ExecuteAsync(insertquery, new
                    {
                        id = input.Id,
                        val = item.IncrementBy
                    });
                    continue;
                }

                var SQL = @$"UPDATE ""{config.LimitsTable}""
                  SET ""{item.FieldName}"" = ""{item.FieldName}"" + @increment, ""UpdatedAt"" = NOW()
                  WHERE ""UserId"" = @id;";

                await connection.ExecuteAsync(SQL, new
                {
                    id = input.Id,
                    increment = item.IncrementBy
                });
            }

            // await transaction.CommitAsync();
        }
        catch
        {
            // await transaction.RollbackAsync();
            throw;
        }
    }
}
