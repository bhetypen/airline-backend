namespace AirlineBackend.Modules.Users.Dtos;

public record RegisterDto(string FirstName, string LastName, string Email, string MobileNo, string Password);
public record LoginDto(string Email, string Password);
public record UpdatePasswordDto(string NewPassword);
public record PublicUserDto(string Id, string Email, string FirstName, string LastName, bool IsAdmin);
