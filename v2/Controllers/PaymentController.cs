using Microsoft.AspNetCore.Mvc;
using v2.Models;
using v2.Services;
using v2.Security;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;
    private readonly IAuthService _authService;

    public PaymentController(IPaymentService service, IAuthService authService)
    {
        _service = service;
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

        var payments = await _service.GetByInitiatorAsync(username);
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var username = _authService.GetCurrentUsername();
        if (username == null)
        {
            return Unauthorized(new { error = "No active user session" });
        }

        var payment = await _service.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        if (payment.Initiator != username)
        {
            return Forbid();
        }

        return Ok(payment);
    }

    [AdminOnly]
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var payments = await _service.GetByInitiatorAsync(username);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
    {
        var payment = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [AdminOnly]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("unpaid/{licensePlate}")]
    public async Task<IActionResult> GetUnpaidSessions(string licensePlate)
    {
        var sessions = await _service.GetUnpaidSessionsAsync(licensePlate);
        return Ok(sessions);
    }

    [AdminOnly]
    [HttpPost("pay-session")]
    public async Task<IActionResult> PaySingleSession([FromBody] PaySingleSessionDto dto)
    {
        var payment = await _service.PaySingleSessionAsync(dto);
        return Ok(payment);
    }
}