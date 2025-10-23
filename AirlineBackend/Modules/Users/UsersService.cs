using AirlineBackend.Common.Auth;
using AirlineBackend.Modules.Users.Dtos;
using AirlineBackend.Modules.Users.Models;
using MongoDB.Driver;

namespace AirlineBackend.Modules.Users;

public class UsersService
{
    private readonly UsersRepository _repo;
    private readonly IAuthService _auth;
    public UsersService(UsersRepository repo, IAuthService auth) { _repo = repo; _auth = auth; }

    public async Task<(int status, object body)> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return (400, new { error = "Email invalid" });

        if (string.IsNullOrWhiteSpace(dto.MobileNo) ||
            !System.Text.RegularExpressions.Regex.IsMatch(dto.MobileNo, "^09\\d{9}$"))
            return (400, new { error = "Mobile number invalid" });

        if (string.IsNullOrEmpty(dto.Password) || dto.Password.Length < 8)
            return (400, new { error = "Password must be atleast 8 characters" });

        var u = new User {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            Email     = dto.Email,
            MobileNo  = dto.MobileNo,
            Password  = BCrypt.Net.BCrypt.HashPassword(dto.Password, 10),
            IsAdmin   = false
        };

        try
        {
            await _repo.CreateAsync(u);
            return (201, new { message = "Registered Successfully" });
        }
        catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return (409, new { error = "Email already exists" });
        }
        catch
        {
            return (500, new { error = "Registration failed" });
        }
    }

    public async Task<(int status, object body)> Login(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return (400, new { error = "Invalid Email" });

        var user = await _repo.FindByEmailAsync(dto.Email);
        if (user is null) return (404, new { error = "No Email Found" });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password ?? "", user.Password))
            return (401, new { error = "Email and password do not match" });

        var access = _auth.CreateAccessToken(user);
        return (200, new { access });
    }
}
