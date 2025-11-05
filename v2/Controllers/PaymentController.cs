using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentController(IPaymentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _service.GetAllAsync();
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _service.GetByIdAsync(id);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpGet("initiator/{initiator}")]
    public async Task<IActionResult> GetByInitiator(string initiator)
    {
        var payments = await _service.GetByInitiatorAsync(initiator);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Payment payment)
    {
        var created = await _service.CreateAsync(payment);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}