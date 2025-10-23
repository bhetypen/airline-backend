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
        services.AddSingleton(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            var name = cfg["Mongo:UsersCollection"] ?? "users";
            return new UsersRepository(db, name);
        });
        services.AddScoped<UsersService>();
        return services;
    }
}
