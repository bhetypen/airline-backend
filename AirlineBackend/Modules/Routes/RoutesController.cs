using AirlineBackend.Modules.Routes.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBackend.Modules.Routes;

[ApiController]
[Route("routes")]
public class RoutesController : ControllerBase
{
    private readonly RoutesService _svc;
    public RoutesController(RoutesService svc) { _svc = svc; }

    // Public reads
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (s, b) = await _svc.GetById(id);
        return StatusCode(s, b);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? origin, [FromQuery] string? destination,
        [FromQuery] bool? active = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (s, b) = await _svc.Search(origin, destination, active, page, pageSize);
        return StatusCode(s, b);
    }

    // Admin writes
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateRouteDto dto)
    {
        var (s, b) = await _svc.Create(dto ?? throw new ArgumentNullException(nameof(dto)));
        return StatusCode(s, b);
    }

    [HttpPatch("{id:length(24)}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRouteDto dto)
    {
        var (s, b) = await _svc.Update(id, dto ?? throw new ArgumentNullException(nameof(dto)));
        return StatusCode(s, b);
    }

    [HttpDelete("{id:length(24)}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        var (s, b) = await _svc.Delete(id);
        return StatusCode(s, b);
    }
}