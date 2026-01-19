using Microsoft.AspNetCore.Mvc;
using v2.Services;
using v2.Security;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _service;
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;

        public VehicleController(IVehicleService service, IAuthService authService, IUserProfileService userProfileService)
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

            var vehicles = await _service.GetByUserIdAsync(user.Id);
            return Ok(vehicles);
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

            var vehicle = await _service.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (vehicle.UserId != user.Id)
            {
                return Forbid();
            }

            return Ok(vehicle);
        }

        [AdminOnly]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var vehicles = await _service.GetByUserIdAsync(userId);
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
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

            var vehicle = new Vehicle
            {
                UserId = user.Id,
                LicensePlate = dto.LicensePlate,
                Make = dto.Make,
                Model = dto.Model,
                Color = dto.Color,
                Year = dto.Year
            };

            var created = await _service.CreateAsync(vehicle);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [AdminOnly]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vehicle updated)
        {
            var result = await _service.UpdateAsync(id, updated);
            return Ok(result);
        }

        [AdminOnly]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}