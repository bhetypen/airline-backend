using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Airports;

public static class AirportsModule
{
    public static IServiceCollection AddAirportsModule(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            var name = cfg["Mongo:AirportsCollection"] ?? "airports";
            return new AirportsRepository(db, name);
        });
        services.AddScoped<AirportsService>();
        return services;
    }
}