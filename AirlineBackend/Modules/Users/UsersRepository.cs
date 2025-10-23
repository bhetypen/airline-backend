using AirlineBackend.Modules.Users.Models;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Users;

public class UsersRepository
{
    private readonly IMongoCollection<User> _col;

    public UsersRepository(IMongoDatabase db, string collectionName)
    {
        _col = db.GetCollection<User>(collectionName);

        // Unique index on email
        _col.Indexes.CreateOne(new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true }
        ));
    }

    public async Task<User?> FindByEmailAsync(string email) 
        => await _col.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<User?> FindByIdAsync(string id)
        => await _col.Find(u => u.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(User u) => _col.InsertOneAsync(u);

    public async Task<bool> SetPasswordAsync(string id, string hashed)
    {
        var up = Builders<User>.Update.Set(u => u.Password, hashed);
        var res = await _col.UpdateOneAsync(u => u.Id == id, up);
        return res.ModifiedCount > 0;
    }

    public async Task<User?> SetAdminAsync(string id, bool isAdmin) 
        => await _col.FindOneAndUpdateAsync(u => u.Id == id,
            Builders<User>.Update.Set(u => u.IsAdmin, isAdmin),
            new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

    public async Task<List<User>> SearchAsync(string q, int limit)
    {
        var f = Builders<User>.Filter;
        var ors = new List<FilterDefinition<User>>
        {
            f.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(q, "i")),
            f.Regex(u => u.FirstName, new MongoDB.Bson.BsonRegularExpression(q, "i")),
            f.Regex(u => u.LastName, new MongoDB.Bson.BsonRegularExpression(q, "i")),
        };
        if (System.Text.RegularExpressions.Regex.IsMatch(q, "^[a-f\\d]{24}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            ors.Add(f.Eq(u => u.Id, q));

        return await _col.Find(f.Or(ors))
            .Limit(Math.Clamp(limit, 1, 25))
            .ToListAsync();
    }
}
