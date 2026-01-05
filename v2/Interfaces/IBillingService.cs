using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IBillingService
    {
        Task<Billing?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Billing>> GetAllAsync();
    }
}