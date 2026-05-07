using Microsoft.Extensions.DependencyInjection;
using OnLimit.DependencyInjection;
using OnLimit.Interfaces;
using OnLimit.MongoDB.Repositories;

namespace OnLimit.MongoDB;

public static class InjectOnLimitMongoDB
{
    public static OnLimitServicesBuilder AddMongoDB(
        this OnLimitServicesBuilder builder,
        OnLimitMongoDBConfiguration? config = null
      )
    {
        builder.Services.AddSingleton<OnLimitMongoDBConfiguration>(config ?? new());
        builder.Services.AddSingleton<IUsageRepository, MongoUsageRepository>();

        return builder;
    }
}
