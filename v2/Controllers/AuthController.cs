using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userService;

        public AuthController(IAuthService authService, IUserProfileService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);

                return Ok(new
                {
                    message = "Registered and logged in successfully.",
                    token = response.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var checkUser = await _userService.GetByUsernameAsync(request.Username);
            if (checkUser == null)
                return BadRequest(new { error = "User does not exist." });

            try
            {
                var response = await _authService.LoginAsync(request);

                return Ok(new
                {
                    message = "Logged in successfully.",
                    token = response.Token
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        // LOGOUT
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            bool success;
            //met meegegeven header
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = Uri.UnescapeDataString(authHeader.Substring("Bearer ".Length).Trim());
                success = await _authService.LogoutAsync(token);
            }
            //automatisch in memory header
            else
            {
                success = await _authService.LogoutCurrentUserAsync();
            }

            if (!success)
                return Unauthorized("No active session or invalid token.");

            return Ok(new { message = "Logged out successfully." });
        }
    }
}
