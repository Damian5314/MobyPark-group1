using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IBillingService
    {
        Task<Billing?> GetByUserIdAsync(int userId);
        Task<Billing?> GetByUsernameAsync(string username);
        Task<IEnumerable<Billing>> GetAllAsync();
    }
}