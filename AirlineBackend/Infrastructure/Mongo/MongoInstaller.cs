using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AirlineBackend.Infrastructure.Mongo;

public static class MongoInstaller
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg["Mongo:ConnectionString"]!;
        var db = cfg["Mongo:Database"]!;
        services.AddSingleton<IMongoClient>(_ => new MongoClient(cs));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(db));
        return services;
    }
}
