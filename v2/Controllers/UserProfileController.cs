using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Security;
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

        // DTO for changing password
        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; } = "";
            public string NewPassword { get; set; } = "";
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

        // PUT /api/UserProfile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateMyProfileDto updateRequest)
        {
            var loggedIn = GetLoggedInUser();
            if (string.IsNullOrWhiteSpace(loggedIn))
                return Unauthorized(new { error = "Missing or invalid token." });

            var updated = await _userService.UpdateAsync(loggedIn, updateRequest);

            if (updated == null)
                return NotFound(new { error = "Profile not found." });

            return Ok(new
            {
                message = "Profile updated successfully.",
                profile = updated
            });
        }

        // PUT /api/UserProfile/me/password
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangeOwnPassword([FromBody] ChangePasswordDto dto)
        {
            var loggedIn = GetLoggedInUser();
            if (string.IsNullOrWhiteSpace(loggedIn))
                return Unauthorized(new { error = "Missing or invalid token." });

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { error = "Both current and new password are required." });

            // IMPORTANT: if username was changed, old tokens may still contain the old username.
            // In that case the user won't be found and you should force re-login.
            var user = await _userService.GetByUsernameAsync(loggedIn);
            if (user == null)
                return Unauthorized(new { error = "Token user not found. Please log in again." });

            var success = await _userService.ChangePasswordAsync(loggedIn, dto.CurrentPassword, dto.NewPassword);

            if (!success)
                return BadRequest(new { error = "Current password is incorrect." });

            return Ok(new { message = "Password changed successfully." });
        }

        // PUT /api/UserProfile/{username}
        // ADMIN ONLY - can update everything incl. password in one body
        [AdminOnly]
        [HttpPut("{username}")]
        public async Task<IActionResult> AdminUpdateUser(string username, [FromBody] AdminUpdateUserDto updateRequest)
        {
            var loggedIn = GetLoggedInUser();
            if (string.IsNullOrWhiteSpace(loggedIn))
                return Unauthorized(new { error = "Missing or invalid token." });

            if (!await IsAdmin(loggedIn))
                return StatusCode(403, new { error = "Only admin can update users." });

            try
            {
                var updated = await _userService.AdminUpdateAsync(username, updateRequest);
                if (updated == null)
                    return NotFound(new { error = "User not found." });
                return Ok(new
                {
                    message = "User updated successfully.",
                    profile = updated
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("taken"))
            {
                return Conflict(new { error = "Username is already taken." });
            }
        }



        // GET /api/UserProfile/{username}
        [AdminOnly]
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

        // DELETE /api/UserProfile/{username}
        [AdminOnly]
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
