using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class BillingService : IBillingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BillingService> _logger;

        public BillingService(AppDbContext context, ILogger<BillingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //by userid
        public async Task<Billing?> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching billing information for user ID: {UserId}", userId);

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for billing request", userId);
                    return null;
                }

                //payments per user
                var payments = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Initiator == user.Username)
                    .ToListAsync();

                if (!payments.Any())
                {
                    _logger.LogInformation("No payments found for user ID: {UserId}", userId);
                    return null;
                }

                _logger.LogInformation("Retrieved {PaymentCount} payments for user ID: {UserId}", payments.Count, userId);

                return new Billing
                {
                    User = user.Username,
                    Payments = payments
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing information for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Billing>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all billing records");

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

                _logger.LogInformation("Retrieved billing records for {UserCount} users", groupedBillings.Count);

                return groupedBillings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all billing records");
                throw;
            }
        }
    }
}