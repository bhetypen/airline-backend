using AirlineBackend.Modules.Users.Models;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Users;

public interface IUsersRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string id);
    Task CreateAsync(User u);
    Task<bool> SetPasswordAsync(string id, string hashed);
    Task<User?> SetAdminAsync(string id, bool isAdmin);
    Task<List<User>> SearchAsync(string q, int limit);
}