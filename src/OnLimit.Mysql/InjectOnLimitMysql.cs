using Microsoft.Extensions.DependencyInjection;

using OnLimit.DependencyInjection;
using OnLimit.Interfaces;
using OnLimit.Mysql.Repositories;

namespace OnLimit.Mysql;

public static class InjectOnLimitMysql
{
    public static OnLimitServicesBuilder AddMysql(
        this OnLimitServicesBuilder builder,
        OnLimitMysqlConfig? config = null
      )
    {
        builder.Services.AddSingleton(config ?? new());
        builder.Services.AddTransient<IUsageRepository, MysqlUsageRepository>();

        return builder;
    }

}