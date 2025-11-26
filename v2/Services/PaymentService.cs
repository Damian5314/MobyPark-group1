using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PaymentService : IPaymentService
{
    private readonly List<Payment> _payments = new();

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return _payments;
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return _payments.FirstOrDefault(p => p.Id == id);
    }

    public async Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator)
    {
        return _payments.Where(p => p.Initiator.Equals(initiator, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        payment.Id = _payments.Count + 1;
        payment.CreatedAt = DateTime.UtcNow;
        payment.Completed = DateTime.UtcNow.AddMinutes(1); // simulate completion
        payment.Transaction = Guid.NewGuid().ToString("N");
        payment.Hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        _payments.Add(payment);
        return payment;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        if (payment == null) return false;

        _payments.Remove(payment);
        return true;
    }
}