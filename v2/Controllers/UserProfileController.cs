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
        private readonly IAuthService _authService;

        public UserProfileController(IUserProfileService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        private string? GetLoggedInUser()
        {
            var header = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer "))
                return null;

            var token = header.Substring("Bearer ".Length).Trim();
            return _authService.GetUsernameFromToken(token);
        }

        private async Task<bool> IsAdmin(string username)
        {
            var user = await _userService.GetByUsernameAsync(username);
            return user != null && user.Role == "ADMIN";
        }


        // GET /api/UserProfile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetOwnProfile()
        {
            var loggedIn = GetLoggedInUser();
            if (loggedIn == null)
                return Unauthorized(new { error = "Missing or invalid token." });

            var user = await _userService.GetByUsernameAsync(loggedIn);
            return user == null ? NotFound() : Ok(user);
        }


        // GET /api/UserProfile/{username}  (ADMIN ONLY)
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var loggedIn = GetLoggedInUser();
            if (loggedIn == null)
                return Unauthorized(new { error = "Missing or invalid token." });

            if (!await IsAdmin(loggedIn))
                return StatusCode(403, new { error = "Only admin can view other user profiles." });

            var user = await _userService.GetByUsernameAsync(username);
            return user == null ? NotFound() : Ok(user);
        }


        // DELETE /api/UserProfile/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteOwnProfile()
        {
            var loggedIn = GetLoggedInUser();
            if (loggedIn == null)
                return Unauthorized(new { error = "Missing or invalid token." });

            var deleted = await _userService.DeleteAsync(loggedIn);
            return deleted ? Ok("Account deleted") : NotFound("Account not found");
        }


        // DELETE /api/UserProfile/{username}  (ADMIN ONLY)
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var loggedIn = GetLoggedInUser();
            if (loggedIn == null)
                return Unauthorized(new { error = "Missing or invalid token." });

            if (!await IsAdmin(loggedIn))
                return StatusCode(403, new { error = "Only admin can delete other users." });

            var deleted = await _userService.DeleteAsync(username);
            return deleted ? NoContent() : NotFound();
        }
    }
}
