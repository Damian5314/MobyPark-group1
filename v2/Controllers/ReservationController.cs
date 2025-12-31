using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;
using v2.Security;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationController(IReservationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reservations = await _service.GetAllAsync();
        return Ok(reservations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _service.GetByIdAsync(id);
        return reservation == null ? NotFound() : Ok(reservation);
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
    public async Task<IActionResult> Update(int id, [FromBody] ReservationCreateDto dto)
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

    //parking session from a reservation
    [HttpPost("{id}/start-session")]
    public async Task<IActionResult> StartSession(
        int id,
        [FromBody] StartSessionFromReservationDto dto)
    {
        var session = await _service.StartSessionFromReservationAsync(
            id,
            dto.LicensePlate,
            dto.Username);

        return Ok(session);
    }
}