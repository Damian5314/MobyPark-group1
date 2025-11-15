using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userService;

        public UserProfileController(IUserProfileService userService)
        {
            _userService = userService;
        }

        // ───────────────────────────────────────────────
        // ADMIN ONLY: GET ALL USERS
        // ───────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // ───────────────────────────────────────────────
        // GET PROFILE
        // Admin → can get anyone
        // User  → can get only their own
        // ───────────────────────────────────────────────
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var currentUsername = User.Identity?.Name;
            var isAdmin = User.IsInRole("ADMIN");

            if (!isAdmin && !string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var user = await _userService.GetByUsernameAsync(username);
            return user == null ? NotFound() : Ok(user);
        }

        // ───────────────────────────────────────────────
        // UPDATE PROFILE (NO PASSWORD HERE)
        // Admin → can update anyone fully
        // User  → can update only their own and CANNOT change Role/Active
        // ───────────────────────────────────────────────
        [HttpPut("{username}")]
        public async Task<IActionResult> Update(string username, [FromBody] UserProfile profile)
        {
            var currentUsername = User.Identity?.Name;
            var isAdmin = User.IsInRole("ADMIN");

            if (!isAdmin && !string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var existing = await _userService.GetByUsernameAsync(username);
            if (existing == null)
                return NotFound();

            // Build safe update object
            var safeUpdate = new UserProfile
            {
                Id = existing.Id,
                Username = existing.Username,
                Password = existing.Password,  // not changed here

                Name = profile.Name,
                Email = profile.Email,
                Phone = profile.Phone,
                BirthYear = profile.BirthYear,

                CreatedAt = existing.CreatedAt,

                // Only admin can change role / active
                Role = isAdmin ? profile.Role : existing.Role,
                Active = isAdmin ? profile.Active : existing.Active
            };

            var updated = await _userService.UpdateAsync(username, safeUpdate);
            return Ok(updated);
        }

        // ───────────────────────────────────────────────
        // CHANGE PASSWORD
        // User → can change their own (must provide current pw)
        // Admin → can reset any user's password
        // ───────────────────────────────────────────────
        [HttpPut("{username}/password")]
        public async Task<IActionResult> ChangePassword(
            string username,
            [FromBody] ChangePasswordRequest request)
        {
            var currentUsername = User.Identity?.Name;
            var isAdmin = User.IsInRole("ADMIN");

            // Only admin or owner
            if (!isAdmin && !string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            // Owner or non-admin: must provide current password
            if (string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase) || !isAdmin)
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                    return BadRequest("Current password is required.");

                var result = await _userService.ChangePasswordAsync(username, request.CurrentPassword, request.NewPassword);
                if (!result)
                    return BadRequest("Incorrect current password.");

                return NoContent();
            }

            // Admin resetting someone else's password
            var resetResult = await _userService.SetPasswordAsync(username, request.NewPassword);
            if (!resetResult)
                return NotFound();

            return NoContent();
        }

        // ───────────────────────────────────────────────
        // DELETE PROFILE
        // Admin → can delete anyone
        // User  → can delete only their own
        // ───────────────────────────────────────────────
        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            var currentUsername = User.Identity?.Name;
            var isAdmin = User.IsInRole("ADMIN");

            if (!isAdmin && !string.Equals(currentUsername, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var deleted = await _userService.DeleteAsync(username);
            return deleted ? NoContent() : NotFound();
        }
    }
}
