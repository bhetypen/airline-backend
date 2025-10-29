namespace AirlineBackend.Modules.Routes.Dtos
{
    public record CreateRouteDto(
        string Origin,
        string Destination,
        int Distance,
        bool Active = true,
        List<string>? Codeshares = null
    );

    public record UpdateRouteDto(
        int? DistanceKm,
        bool? Active,
        List<string> Codeshares
        );

    public record RouteListItemDto(
        string Id,
        string Origin,
        string Destination,
        int DistanceKm,
        bool Active,
        List<string> Codeshares
        );
}