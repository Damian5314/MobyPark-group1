using Microsoft.AspNetCore.Authorization;
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
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Missing credentials" });
            }

            var user = await _authService.ValidateUserAsync(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.Now.AddHours(24)
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // JWT logout gebeurt client-side door de token te verwijderen
            // Deze endpoint kan gebruikt worden voor logging of token blacklisting
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
