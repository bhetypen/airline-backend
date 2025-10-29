using AirlineBackend.Modules.Routes.Dtos;
using AirlineBackend.Modules.Routes.Models;
using AirlineBackend.Modules.Airports;
using MongoDB.Driver;

// FIX 1: Alias für das Routenmodell erstellen, um Konflikte zu vermeiden
using RouteModel = AirlineBackend.Modules.Routes.Models.Route;

namespace AirlineBackend.Modules.Routes;

public class RoutesService
{
    private readonly RoutesRepository _routes;
    private readonly AirportsRepository _airports;
    
    public RoutesService(RoutesRepository routes, AirportsRepository airports)
    {
        _routes = routes;
        _airports = airports;
    }

    private static bool isIata(string? s) =>
        !string.IsNullOrWhiteSpace(s) && s.Length == 3;
    
    public async Task<(int status, object body)> Create(CreateRouteDto dto)
    {
        // FIX: isIata ist nun kleingeschrieben
        if (!isIata(dto.Origin) || !isIata(dto.Destination))
            return (400, new { error = "Origin and destination must be 3-letter IATA codes" });

        var origin = dto.Origin.ToUpperInvariant();
        var dest   = dto.Destination.ToUpperInvariant();

        if (origin == dest)
            return (400, new { error = "Origin and destination must be different" });

        // Validate airports exist by IATA
        var oa = await _airports.FindByIataAsync(origin);
        var da = await _airports.FindByIataAsync(dest);
        if (oa is null || da is null)
            return (404, new { error = "Origin or destination airport not found" });

        // Unique pair
        if (await _routes.FindByPairAsync(origin, dest) is not null)
            return (409, new { error = "Route already exists" });

        // FIX 3: Route durch RouteModel ersetzen
        var r = new RouteModel
        {
            Origin = origin,
            Destination = dest,
            // FIX 2: Eigenschaft von dto.DistanceKm auf dto.Distance korrigiert
            DistanceKm = dto.Distance, 
            Active = dto.Active,
            Codeshares = (dto.Codeshares ?? new()).Select(c => c.ToUpperInvariant()).ToList(),
            CreatedAt = DateTime.UtcNow
        };

        await _routes.CreateAsync(r);
        return (201, new { id = r.Id });
    }
    
    public async Task<(int status, object body)> Update(string id, UpdateRouteDto dto)
    {
        // FIX 3: Route durch RouteModel in den Buildern ersetzen
        var u = Builders<RouteModel>.Update.Set(r => r.UpdatedAt, DateTime.UtcNow);

        // Die folgenden Fehler (CS1660) waren auf den mehrdeutigen Verweis zurückzuführen.
        // Durch die Verwendung von RouteModel sollten sie behoben sein.
        if (dto.DistanceKm.HasValue)
            u = u.Set(r => r.DistanceKm, dto.DistanceKm.Value);

        if (dto.Active.HasValue)
            u = u.Set(r => r.Active, dto.Active.Value);

        if (dto.Codeshares is not null)
        {
            var list = dto.Codeshares.Select(c => c.ToUpperInvariant()).ToList();
            u = u.Set(r => r.Codeshares, list);
        }

        // FIX 3: RouteModel im FindOneAndUpdateAsync-Aufruf verwenden
        var updated = await _routes.UpdateAsync(id, u);
        if (updated is null) return (404, new { error = "Route not found" });

        return (200, new { id = updated.Id });
    }
    
    public async Task<(int status, object body)> GetById(string id)
    {
        var r = await _routes.FindByIdAsync(id);
        if (r is null) return (404, new { error = "Route not found" });

        return (200, new
        {
            route = new
            {
                id = r.Id,
                r.Origin,
                r.Destination,
                r.DistanceKm,
                r.Active,
                r.Codeshares,
                r.CreatedAt,
                r.UpdatedAt
            }
        });
    }
    
    public async Task<(int status, object body)> Search(string? origin, string? destination, bool? active, int page, int pageSize)
    {
        var (items, total) = await _routes.SearchAsync(origin?.ToUpperInvariant(), destination?.ToUpperInvariant(), active, Math.Max(1, page), Math.Clamp(pageSize, 1, 200));
        return (200, new
        {
            total,
            items = items.Select(r => new
            {
                id = r.Id,
                r.Origin,
                r.Destination,
                r.DistanceKm,
                r.Active,
                r.Codeshares
            })
        });
    }
    
    public async Task<(int status, object body)> Delete(string id)
    {
        var ok = await _routes.DeleteAsync(id);
        return ok ? (200, new { deleted = true }) : (404, new { error = "Route not found" });
    }
}