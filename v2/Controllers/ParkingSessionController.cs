using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;

[ApiController]
[Route("api/[controller]")]
public class ParkingSessionController : ControllerBase
{
    private readonly IParkingSessionService _service;

    public ParkingSessionController(IParkingSessionService service)
    {
        _service = service;
    }

    // Start a parking session
    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
    {
        var session = await _service.StartSessionAsync(dto.ParkingLotId, dto.LicensePlate, dto.Username);
        return Ok(session);
    }

    // Stop a parking session
    [HttpPost("stop/{sessionId}")]
    public async Task<IActionResult> StopSession(int sessionId)
    {
        var session = await _service.StopSessionAsync(sessionId);
        return Ok(session);
    }

    // Get a parking session by ID
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetById(int sessionId)
    {
        var session = await _service.GetByIdAsync(sessionId);
        return session == null ? NotFound() : Ok(session);
    }

    // Get all active sessions
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var sessions = await _service.GetActiveSessionsAsync();
        return Ok(sessions);
    }
}