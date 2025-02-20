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
        builder.services.AddSingleton<OnLimitMongoDBConfiguration>(config ?? new());
        builder.services.AddSingleton<IUsageRepository, MongoUsageRepository>();

        return builder;
    }
}

