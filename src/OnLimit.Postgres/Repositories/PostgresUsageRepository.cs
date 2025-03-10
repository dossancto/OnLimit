using System.Text.Json;
using Dapper;
using Npgsql;
using OnLimit.Entities;
using OnLimit.Interfaces;
using OnLimit.Manager.Impl.Dtos;

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

        var res = await connection.QueryFirstOrDefaultAsync<UsageUserPlans>(SQL, new
        {
            id = Id,
            date = date
        });

        return res;
    }

    public async Task<UsageUserPlans?> GetLatestUserPlan(string Id)
    {
        var query = $"SELECT * FROM \"{config.LinkTable}\" WHERE \"UserId\" = @id ORDER BY \"CreatedAt\" DESC LIMIT 1;";

        return await connection.QueryFirstOrDefaultAsync<UsageUserPlans>(query, new { id = Id });
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

    public async Task SetPlan(string orgId, string plan, DateTime? at = null)
    {
        var now = at ?? DateTime.Now;

        var model = new UsageUserPlans()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = orgId,
            CreatedAt = now,
            Plan = plan,
            Date = UsageUserPlans.MapDate(now),
        };

        var SQL = $@"INSERT INTO ""{config.LinkTable}""
          (""Id"", ""UserId"", ""Plan"", ""Date"", ""CreatedAt"")
          VALUES (@Id, @UserId, @Plan, @Date, @CreatedAt);
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
}
