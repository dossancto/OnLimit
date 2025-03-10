using Microsoft.Extensions.DependencyInjection;
using OnLimit.DependencyInjection;
using OnLimit.Interfaces;
using OnLimit.Postgres.Repositories;

namespace OnLimit.Postgres;

public static class InjectOnLimitPostgres
{
    public static OnLimitServicesBuilder AddPostgres(
        this OnLimitServicesBuilder builder,
        OnLimitPostgresConfig? config = null
      )
    {
        builder.services.AddSingleton<OnLimitPostgresConfig>(config ?? new());
        builder.services.AddTransient<IUsageRepository, PostgresUsageRepository>();

        return builder;
    }

}
