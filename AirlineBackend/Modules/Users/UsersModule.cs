using AirlineBackend.Common.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton<IAuthService, AuthService>();
        
        // REGISTER INTERFACE MAPPING
        services.AddSingleton<IUsersRepository>(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            var name = cfg["Mongo:UsersCollection"] ?? "users";
            // Return the concrete class instance
            return new UsersRepository(db, name);
        });
        
        services.AddScoped<UsersService>();
        return services;
    }
}
