using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ParkingSessionController : ControllerBase
{
    private readonly IParkingSessionService _service;

    public ParkingSessionController(IParkingSessionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _service.GetAllAsync();
        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var session = await _service.GetByIdAsync(id);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetByUser(string username)
    {
        var sessions = await _service.GetByUserAsync(username);
        return Ok(sessions);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] ParkingSession session)
    {
        var started = await _service.StartSessionAsync(session);
        return CreatedAtAction(nameof(GetById), new { id = started.Id }, started);
    }

    [HttpPost("stop/{id}")]
    public async Task<IActionResult> Stop(int id)
    {
        var stopped = await _service.StopSessionAsync(id);
        return Ok(stopped);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}