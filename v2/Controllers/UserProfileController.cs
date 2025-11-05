using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userService;

        public UserProfileController(IUserProfileService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserProfile profile)
        {
            try
            {
                var created = await _userService.CreateAsync(profile);
                return CreatedAtAction(nameof(GetByUsername), new { username = created.Username }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> Update(string username, [FromBody] UserProfile profile)
        {
            var updated = await _userService.UpdateAsync(username, profile);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            var deleted = await _userService.DeleteAsync(username);
            return deleted ? NoContent() : NotFound();
        }
    }
}