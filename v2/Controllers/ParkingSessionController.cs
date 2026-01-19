using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;
using v2.Security;

[ApiController]
[Route("api/[controller]")]
public class ParkingSessionController : ControllerBase
{
    private readonly IParkingSessionService _service;
    private readonly IAuthService _authService;
    private readonly IUserProfileService _userProfileService;

    public ParkingSessionController(IParkingSessionService service, IAuthService authService, IUserProfileService userProfileService)
    {
        _service = service;
        _authService = authService;
        _userProfileService = userProfileService;
    }


    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
    {
        var session = await _service.StartSessionAsync(dto.ParkingLotId, dto.LicensePlate, dto.Username);
        return Ok(session);
    }


    [HttpPost("stop/{sessionId}")]
    public async Task<IActionResult> StopSession(int sessionId)
    {
        var session = await _service.StopSessionAsync(sessionId);
        return Ok(session);
    }

    [AdminOnly]
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetById(int sessionId)
    {
        var session = await _service.GetByIdAsync(sessionId);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var username = _authService.GetCurrentUsername();
        if (username == null)
        {
            return Unauthorized(new { error = "No active user session" });
        }

        var user = await _userProfileService.GetByUsernameAsync(username);
        if (user == null)
        {
            return Unauthorized(new { error = "User not found" });
        }

        var sessions = await _service.GetActiveSessionsByUserIdAsync(user.Id);
        return Ok(sessions);
    }
}