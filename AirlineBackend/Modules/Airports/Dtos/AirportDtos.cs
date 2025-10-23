namespace AirlineBackend.Modules.Airports.Dtos;

public record CreateAirportDto(
    string Iata, string Icao, string Name, string City, string Country,
    double? Lat, double? Lng, string? TimeZone, bool Active = true);

public record UpdateAirportDto(
    string? Name, string? City, string? Country,
    double? Lat, double? Lng, string? TimeZone, bool? Active);

public record AirportListItemDto(
    string Id, string Iata, string Icao, string Name, string City, string Country, bool Active);