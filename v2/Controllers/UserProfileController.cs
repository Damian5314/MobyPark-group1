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

        // HELPER: Get username from token
        private string? GetLoggedInUsername()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader.Replace("Bearer ", "");

            if (!_authService.IsTokenValid(token))
                return null;

            return _authService.GetUsernameFromToken(token);
        }

        private bool IsAdmin(string username)
        {
            var user = _userService.GetByUsernameAsync(username).Result;
            return user != null && user.Role == "ADMIN";
        }

        // USER: GET OWN PROFILE
        // GET /api/UserProfile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetOwnProfile()
        {
            var loggedIn = GetLoggedInUsername();
            if (loggedIn == null)
                return Unauthorized("Invalid or expired token.");

            var user = await _userService.GetByUsernameAsync(loggedIn);
            return user == null ? NotFound() : Ok(user);
        }

        // ADMIN: GET ANY USER PROFILE
        // GET /api/UserProfile/{username}
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            var loggedIn = GetLoggedInUsername();
            if (loggedIn == null)
                return Unauthorized("Invalid or expired token.");

            if (!IsAdmin(loggedIn))
                return Forbid();

            var user = await _userService.GetByUsernameAsync(username);
            return user == null ? NotFound() : Ok(user);
        }

        // USER: DELETE OWN PROFILE
        // DELETE /api/UserProfile/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteOwnProfile()
        {
            var loggedIn = GetLoggedInUsername();
            if (loggedIn == null)
                return Unauthorized("Invalid or expired token.");

            var deleted = await _userService.DeleteAsync(loggedIn);
            return deleted ? NoContent() : NotFound();
        }

        // ADMIN: DELETE ANY USER
        // DELETE /api/UserProfile/{username}
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var loggedIn = GetLoggedInUsername();
            if (loggedIn == null)
                return Unauthorized("Invalid or expired token.");

            if (!IsAdmin(loggedIn))
                return Forbid();

            var deleted = await _userService.DeleteAsync(username);
            return deleted ? NoContent() : NotFound();
        }
    }
}
