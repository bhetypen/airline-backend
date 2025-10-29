using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AirlineBackend.Modules.Routes.Models;

public class Route
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required, StringLength(3, MinimumLength = 3)]
    public string Origin { get; set; } = ""; //IATA code
    
    [Required, StringLength(3, MinimumLength = 3)]
    public string Destination { get; set; } = "";
    
    public int DistanceKm { get; set; }
    
    public bool Active { get; set; }

    public List<string> Codeshares { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }


}