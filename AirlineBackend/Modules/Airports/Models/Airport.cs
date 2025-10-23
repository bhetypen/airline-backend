using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AirlineBackend.Modules.Airports.Models;

public class Airport
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Iata { get; set; } = "";     // e.g., MNL

    [Required, StringLength(4, MinimumLength = 4)]
    public string Icao { get; set; } = "";     // e.g., RPLL

    [Required] public string Name { get; set; } = "";
    [Required] public string City { get; set; } = "";
    [Required] public string Country { get; set; } = "";

    // optional metadata
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? TimeZone { get; set; }      // e.g., "Asia/Manila"
    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}