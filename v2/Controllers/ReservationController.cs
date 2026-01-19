using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;
using v2.Security;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IAuthService _authService;
    private readonly IUserProfileService _userProfileService;

    public ReservationController(IReservationService service, IAuthService authService, IUserProfileService userProfileService)
    {
        _service = service;
        _authService = authService;
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
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

        var reservations = await _service.GetByUserIdAsync(user.Id);
        return Ok(reservations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
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

        var reservation = await _service.GetByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.UserId != user.Id)
        {
            return Forbid();
        }

        return Ok(reservation);
    }

    [AdminOnly]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [AdminOnly]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(
        int id,
        [FromBody] ReservationCreateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [AdminOnly]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

}