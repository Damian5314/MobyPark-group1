using Microsoft.AspNetCore.Mvc;
using v2.Security;
using v2.Services;

namespace v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly IAuthService _authService;

        public BillingController(IBillingService billingService, IAuthService authService)
        {
            _billingService = billingService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var username = _authService.GetCurrentUsername();
            if (username == null)
            {
                return Unauthorized(new { error = "No active user session" });
            }

            var billing = await _billingService.GetByUsernameAsync(username);
            return billing == null ? Ok(new { User = username, Payments = new List<object>() }) : Ok(billing);
        }

        [AdminOnly]
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var billing = await _billingService.GetByUserIdAsync(userId);
            return billing == null ? NotFound() : Ok(billing);
        }
    }
}