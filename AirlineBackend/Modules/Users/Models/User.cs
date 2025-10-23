using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AirlineBackend.Modules.Users.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";
    [Required] public string Email     { get; set; } = "";
    [Required] public string Password  { get; set; } = ""; // hashed
    public bool IsAdmin { get; set; } = false;
    [Required] public string MobileNo  { get; set; } = "";
}
