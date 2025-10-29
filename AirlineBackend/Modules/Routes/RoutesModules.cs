using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Routes;

public static class RoutesModule
{
    public static IServiceCollection AddRoutesModule(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            var name = cfg["Mongo:RoutesCollection"] ?? "routes";
            return new RoutesRepository(db, name);
        });

        services.AddScoped<RoutesService>();
        return services;
    }
}