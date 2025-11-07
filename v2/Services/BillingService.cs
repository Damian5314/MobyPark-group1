using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class BillingService : IBillingService
    {
        private readonly AppDbContext _context;

        public BillingService(AppDbContext context)
        {
            _context = context;
        }

        // Get billing by user ID
        public async Task<Billing?> GetByUserIdAsync(int userId)
        {
            // Fetch the user from the Users table
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            // Fetch payments for this user
            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Initiator == user.Username)
                .ToListAsync();

            if (!payments.Any())
                return null;

            return new Billing
            {
                User = user.Username,
                Payments = payments
            };
        }

        // Get all billing summaries (up to 50)
        public async Task<IEnumerable<Billing>> GetAllAsync()
        {
            var groupedBillings = await _context.Payments
                .AsNoTracking()
                .GroupBy(p => p.Initiator)
                .Select(g => new Billing
                {
                    User = g.Key,
                    Payments = g.ToList()
                })
                .Take(100)
                .ToListAsync();

            return groupedBillings;
        }
    }
}