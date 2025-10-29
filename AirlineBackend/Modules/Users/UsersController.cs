using AirlineBackend.Modules.Users.Dtos;
using AirlineBackend.Modules.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AirlineBackend.Modules.Users;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UsersService _service;
    private readonly IUsersRepository _repo;

    public UsersController(UsersService service, IUsersRepository repo)
    { _service = service; _repo = repo; }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (status, body) = await _service.Register(dto);
        return StatusCode(status, body);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (status, body) = await _service.Login(dto);
        return StatusCode(status, body);
    }

    [HttpGet("details")]
    [Authorize]
    public async Task<IActionResult> Details()
    {
        var id = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(id)) return NotFound(new { error = "User not found" });
        var user = await _repo.FindByIdAsync(id);
        if (user is null) return NotFound(new { error = "User not found" });

        return Ok(new
        {
            user = new PublicUserDto(
                user.Id!,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsAdmin)
        });
    }
    
    [HttpPatch("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto body)
    {
        var id = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(id)) return NotFound(new { error = "User not found" });
        
        var ok = await _repo.SetPasswordAsync(id, body.NewPassword);
        if (!ok) return NotFound(new { error = "User not found" });

        return StatusCode(201, new { message = "Password reset successfully" });
    }
    

    [HttpPatch("{id:length(24)}/set-as-admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetAsAdmin([FromRoute] string id, [FromBody] SetAdminDto? body)
     {
         bool makeAdmin = body?.MakeAdmin ?? true;
     
         var updated = await _repo.SetAdminAsync(id, makeAdmin);
         if (updated is null)
             return NotFound(new { error = "User not Found" });
     
         return Ok(new
         {
             updatedUser = new PublicUserDto(
                 updated.Id!,
                 updated.Email,
                 updated.FirstName,
                 updated.LastName,
                 updated.IsAdmin)
         });
     }

    [HttpGet("search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int limit = 10)
    {
        var query = (q ?? "").Trim();
        if (string.IsNullOrEmpty(query)) return Ok(Array.Empty<object>());
        
        var users = await _repo.SearchAsync(query, limit);
        
        return Ok(users.Select(u => new PublicUserDto(
            u.Id!,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsAdmin
        )));
    }
}
