using MongoDB.Driver;
using RouteModel = AirlineBackend.Modules.Routes.Models.Route;

namespace AirlineBackend.Modules.Routes;

public class RoutesRepository
{
    private readonly IMongoCollection<RouteModel> _col;

    public RoutesRepository(IMongoDatabase db, string collectionName)
    {
        _col = db.GetCollection<RouteModel>(collectionName);

        var indexes = new List<CreateIndexModel<RouteModel>>
        {
            new(
                Builders<RouteModel>.IndexKeys
                    .Ascending(r => r.Origin)
                    .Ascending(r => r.Destination),
                new CreateIndexOptions { Unique = true }
            ),
            new(Builders<RouteModel>.IndexKeys.Ascending(r => r.Active))
        };

        _col.Indexes.CreateMany(indexes);
    }

    public async Task<RouteModel?> FindByIdAsync(string id) =>
        await _col.Find(r => r.Id == id).FirstOrDefaultAsync();

    public async Task<RouteModel?> FindByPairAsync(string originIata, string destIata)
    {
        var o = originIata.ToUpperInvariant();
        var d = destIata.ToUpperInvariant();
        return await _col.Find(r => r.Origin == o && r.Destination == d).FirstOrDefaultAsync();
    }

    public Task CreateAsync(RouteModel r) => _col.InsertOneAsync(r);

    public async Task<RouteModel?> UpdateAsync(string id, UpdateDefinition<RouteModel> update) =>
        await _col.FindOneAndUpdateAsync(
            r => r.Id == id,
            update,
            new FindOneAndUpdateOptions<RouteModel> { ReturnDocument = ReturnDocument.After });

    public async Task<bool> DeleteAsync(string id)
    {
        var res = await _col.DeleteOneAsync(r => r.Id == id);
        return res.DeletedCount > 0;
    }

    public async Task<(List<RouteModel> items, long total)> SearchAsync(
        string? origin, string? destination, bool? active, int page, int pageSize)
    {
        var f = Builders<RouteModel>.Filter;
        var filters = new List<FilterDefinition<RouteModel>>();

        if (!string.IsNullOrWhiteSpace(origin))
            filters.Add(f.Eq(r => r.Origin, origin.ToUpperInvariant()));
        if (!string.IsNullOrWhiteSpace(destination))
            filters.Add(f.Eq(r => r.Destination, destination.ToUpperInvariant()));
        if (active.HasValue)
            filters.Add(f.Eq(r => r.Active, active.Value));

        var filter = filters.Count > 0 ? f.And(filters) : f.Empty;

        var find = _col.Find(filter);
        var total = await find.CountDocumentsAsync();
        var items = await find
            .SortBy(r => r.Origin).ThenBy(r => r.Destination)
            .Skip(Math.Max(0, (page - 1) * pageSize))
            .Limit(Math.Clamp(pageSize, 1, 200))
            .ToListAsync();

        return (items, total);
    }
}
