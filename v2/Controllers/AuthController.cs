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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

        // LOGOUT (token required in HEADER)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("Missing or invalid token.");

            var token = Uri.UnescapeDataString(authHeader.Substring("Bearer ".Length).Trim());

            bool success = await _authService.LogoutAsync(token);

            if (!success)
                return Unauthorized("Invalid or expired token.");

            return Ok(new { message = "Logged out successfully." });
        }

    }
}
