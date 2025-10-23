using AirlineBackend.Modules.Airports.Models;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Airports;

public class AirportsRepository
{
    private readonly IMongoCollection<Airport> _col;

    public AirportsRepository(IMongoDatabase db, string collectionName)
    {
        _col = db.GetCollection<Airport>(collectionName);

        // unique indexes on IATA/ICAO, plus search helpers
        var models = new List<CreateIndexModel<Airport>> {
            new(Builders<Airport>.IndexKeys.Ascending(a => a.Iata), new CreateIndexOptions { Unique = true }),
            new(Builders<Airport>.IndexKeys.Ascending(a => a.Icao), new CreateIndexOptions { Unique = true }),
            new(Builders<Airport>.IndexKeys
                .Ascending(a => a.City)
                .Ascending(a => a.Country)
                .Ascending(a => a.Name))
        };
        _col.Indexes.CreateMany(models);
    }

    public Task<bool> ExistsByIataAsync(string iata) =>
        _col.Find(a => a.Iata == iata).AnyAsync();

    public Task<bool> ExistsByIcaoAsync(string icao) =>
        _col.Find(a => a.Icao == icao).AnyAsync();

    public async Task<Airport?> FindByIdAsync(string id) =>
        await _col.Find(a => a.Id == id).FirstOrDefaultAsync();

    public async Task<Airport?> FindByIataAsync(string iata) =>
        await _col.Find(a => a.Iata == iata).FirstOrDefaultAsync();

    public async Task CreateAsync(Airport a) => await _col.InsertOneAsync(a);

    public async Task<Airport?> UpdateAsync(string id, UpdateDefinition<Airport> update)
        => await _col.FindOneAndUpdateAsync(a => a.Id == id, update,
            new FindOneAndUpdateOptions<Airport> { ReturnDocument = ReturnDocument.After });

    public async Task<bool> DeleteAsync(string id)
    {
        var res = await _col.DeleteOneAsync(a => a.Id == id);
        return res.DeletedCount > 0;
    }

    public async Task<(List<Airport> items, long total)> SearchAsync(string? q, int page, int pageSize, bool? active)
    {
        var f = Builders<Airport>.Filter;
        var filters = new List<FilterDefinition<Airport>>();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var rx = new MongoDB.Bson.BsonRegularExpression(q, "i");
            filters.Add(f.Or(
                f.Regex(a => a.Iata, rx),
                f.Regex(a => a.Icao, rx),
                f.Regex(a => a.Name, rx),
                f.Regex(a => a.City, rx),
                f.Regex(a => a.Country, rx)
            ));
        }
        if (active.HasValue) filters.Add(f.Eq(a => a.Active, active.Value));
        var filter = filters.Count > 0 ? f.And(filters) : f.Empty;

        var find = _col.Find(filter);
        var total = await find.CountDocumentsAsync();
        var items = await find
            .SortBy(a => a.City).ThenBy(a => a.Name)
            .Skip(Math.Max(0, (page - 1) * pageSize))
            .Limit(Math.Clamp(pageSize, 1, 200))
            .ToListAsync();

        return (items, total);
    }
}
