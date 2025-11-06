using v2.Models;

namespace v2.Services
{
    public class BillingService : IBillingService
    {
        private readonly IPaymentService _paymentService;

        public BillingService(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<Billing?> GetByUserAsync(string username)
        {
            var payments = await _paymentService.GetByInitiatorAsync(username);
            if (payments == null || !payments.Any())
                return null;

            return new Billing
            {
                User = username,
                Payments = payments.ToList()
            };
        }

        public async Task<IEnumerable<Billing>> GetAllAsync()
        {
            var allPayments = await _paymentService.GetAllAsync();

            var grouped = allPayments
                .GroupBy(p => p.Initiator)
                .Select(g => new Billing
                {
                    User = g.Key,
                    Payments = g.ToList()
                });

            return grouped;
        }
    }
}