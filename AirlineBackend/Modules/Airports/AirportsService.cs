using AirlineBackend.Modules.Airports.Dtos;
using AirlineBackend.Modules.Airports.Models;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Airports;

public class AirportsService
{
    private readonly AirportsRepository _repo;
    public AirportsService(AirportsRepository repo) { _repo = repo; }

    public async Task<(int status, object body)> Create(CreateAirportDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Iata) || dto.Iata.Length != 3)
            return (400, new { error = "IATA must be 3 letters" });
        if (string.IsNullOrWhiteSpace(dto.Icao) || dto.Icao.Length != 4)
            return (400, new { error = "ICAO must be 4 letters" });

        if (await _repo.ExistsByIataAsync(dto.Iata))
            return (409, new { error = "IATA already exists" });
        if (await _repo.ExistsByIcaoAsync(dto.Icao))
            return (409, new { error = "ICAO already exists" });

        var a = new Airport {
            Iata = dto.Iata.ToUpperInvariant(),
            Icao = dto.Icao.ToUpperInvariant(),
            Name = dto.Name.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            Lat = dto.Lat, Lng = dto.Lng, TimeZone = dto.TimeZone, Active = dto.Active
        };
        await _repo.CreateAsync(a);

        return (201, new { id = a.Id });
    }

    public async Task<(int status, object body)> Update(string id, UpdateAirportDto dto)
    {
        var u = Builders<Airport>.Update
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        if (dto.Name is not null) u = u.Set(a => a.Name, dto.Name.Trim());
        if (dto.City is not null) u = u.Set(a => a.City, dto.City.Trim());
        if (dto.Country is not null) u = u.Set(a => a.Country, dto.Country.Trim());
        if (dto.Lat.HasValue) u = u.Set(a => a.Lat, dto.Lat);
        if (dto.Lng.HasValue) u = u.Set(a => a.Lng, dto.Lng);
        if (dto.TimeZone is not null) u = u.Set(a => a.TimeZone, dto.TimeZone);
        if (dto.Active.HasValue) u = u.Set(a => a.Active, dto.Active.Value);

        var updated = await _repo.UpdateAsync(id, u);
        if (updated is null) return (404, new { error = "Airport not found" });

        return (200, new { id = updated.Id });
    }

    public async Task<(int status, object body)> GetById(string id)
    {
        var a = await _repo.FindByIdAsync(id);
        if (a is null) return (404, new { error = "Airport not found" });

        return (200, new {
            airport = new {
                id = a.Id, a.Iata, a.Icao, a.Name, a.City, a.Country,
                a.Lat, a.Lng, a.TimeZone, a.Active
            }
        });
    }

    public async Task<(int status, object body)> GetByIata(string iata)
    {
        var a = await _repo.FindByIataAsync(iata.ToUpperInvariant());
        if (a is null) return (404, new { error = "Airport not found" });

        return (200, new {
            airport = new {
                id = a.Id, a.Iata, a.Icao, a.Name, a.City, a.Country,
                a.Lat, a.Lng, a.TimeZone, a.Active
            }
        });
    }

    public async Task<(int status, object body)> Search(string? q, int page, int pageSize, bool? active)
    {
        var (items, total) = await _repo.SearchAsync(q?.Trim(), Math.Max(1, page), Math.Clamp(pageSize, 1, 200), active);
        return (200, new {
            total,
            items = items.Select(a => new AirportListItemDto(a.Id!, a.Iata, a.Icao, a.Name, a.City, a.Country, a.Active))
        });
    }

    public async Task<(int status, object body)> Delete(string id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? (200, new { deleted = true }) : (404, new { error = "Airport not found" });
    }
}
