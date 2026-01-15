using Microsoft.AspNetCore.Mvc;
using v2.Security;
using v2.Services;
using v2.Models;


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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lot = await _service.GetByIdAsync(id);
            return lot == null ? NotFound() : Ok(lot);
        }

        [AdminOnly]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ParkingLot lot)
        {
            var created = await _service.CreateAsync(lot);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [AdminOnly]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ParkingLot lot)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, lot);
                return Ok(updated); // Succesvol bijgewerkt
            }
            catch (ArgumentException ex)
            {
                // Ongeldige of ontbrekende velden
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Parkeerplaats bestaat niet
                return NotFound(new { message = ex.Message });
            }
        }

        [AdminOnly]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Parking lot deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}