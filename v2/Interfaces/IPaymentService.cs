using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator);
    Task<Payment> CreateAsync(Payment payment);
    Task<bool> DeleteAsync(int id);
}