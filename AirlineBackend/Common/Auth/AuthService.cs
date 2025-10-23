using AirlineBackend.Modules.Users.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AirlineBackend.Common.Auth;

public interface IAuthService { string CreateAccessToken(User u); }

public class AuthService : IAuthService
{
    private readonly JwtOptions _opt;
    private readonly SymmetricSecurityKey _key;
    
    public AuthService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
    }
    
    public string CreateAccessToken(User u)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id ?? ""),
            new Claim("id", u.Id ?? ""),
            new Claim("email", u.Email),
            new Claim("isAdmin", u.IsAdmin ? "true" : "false")
        };
        
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
        
    }
}