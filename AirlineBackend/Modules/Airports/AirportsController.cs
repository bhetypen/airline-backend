using AirlineBackend.Modules.Airports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBackend.Modules.Airports;

[ApiController]
[Route("airports")]
public class AirportsController : ControllerBase
{
    private readonly AirportsService _svc;
    public AirportsController(AirportsService svc) { _svc = svc; }

    // Public read endpoints
    [HttpGet("{id:length(24)}")]
    public async Task<IActionResult> GetById(string id)
    {
        var (s, b) = await _svc.GetById(id);
        return StatusCode(s, b);
    }

    [HttpGet("iata/{iata:length(3)}")]
    public async Task<IActionResult> GetByIata(string iata)
    {
        var (s, b) = await _svc.GetByIata(iata);
        return StatusCode(s, b);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? active = null)
    {
        var (s, b) = await _svc.Search(q, page, pageSize, active);
        return StatusCode(s, b);
    }

    // Admin write endpoints
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateAirportDto dto)
    {
        var (s, b) = await _svc.Create(dto ?? throw new ArgumentNullException(nameof(dto)));
        return StatusCode(s, b);
    }

    [HttpPatch("{id:length(24)}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAirportDto dto)
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