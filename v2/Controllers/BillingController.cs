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

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [AdminOnly]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _billingService.GetAllAsync();
            return Ok(bills);
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