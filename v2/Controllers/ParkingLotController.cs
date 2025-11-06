using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService _service;

        public ParkingLotController(IParkingLotService service)
        {
            _service = service;
        }

        // ---- CRUD ----

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lot = await _service.GetByIdAsync(id);
            return lot == null ? NotFound() : Ok(lot);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ParkingLot lot)
        {
            var created = await _service.CreateAsync(lot);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ParkingLot lot)
        {
            var updated = await _service.UpdateAsync(id, lot);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // ---- Parking Sessions ----

        [HttpPost("{id}/sessions/start")]
        public async Task<IActionResult> StartSession(int id, [FromQuery] string licensePlate, [FromQuery] string username)
        {
            var session = await _service.StartSessionAsync(id, licensePlate, username);
            return Ok(session);
        }

        [HttpPost("{id}/sessions/stop")]
        public async Task<IActionResult> StopSession(int id, [FromQuery] string licensePlate, [FromQuery] string username)
        {
            var session = await _service.StopSessionAsync(id, licensePlate, username);
            return Ok(session);
        }

        [HttpGet("{id}/sessions")]
        public async Task<IActionResult> GetSessions(int id)
        {
            var sessions = await _service.GetSessionsAsync(id);
            return Ok(sessions);
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var deleted = await _service.DeleteSessionAsync(sessionId);
            return deleted ? NoContent() : NotFound();
        }
    }
}