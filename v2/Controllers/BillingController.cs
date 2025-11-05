using Microsoft.AspNetCore.Mvc;
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

        // ---- Get all users' billing summaries ----
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _billingService.GetAllAsync();
            return Ok(bills);
        }

        // ---- Get billing for specific user ----
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUser(string username)
        {
            var billing = await _billingService.GetByUserAsync(username);
            return billing == null ? NotFound() : Ok(billing);
        }
    }
}