using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.Initiator == initiator)
                .OrderByDescending(p => p.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        // ⭐ THIS IS NOW "PAY OPEN PARKING SESSIONS"
        public async Task<Payment> CreateAsync(Payment payment)
        {
            if (string.IsNullOrWhiteSpace(payment.Initiator))
                throw new InvalidOperationException("Initiator is required");

            // 1️⃣ Find unpaid, stopped parking sessions
            var openSessions = await _context.ParkingSessions
                .Where(s =>
                    s.PaymentStatus == "Pending" &&
                    s.Stopped != default &&
                    s.LicensePlate != null)
                .ToListAsync();

            if (!openSessions.Any())
                throw new InvalidOperationException("No unpaid parking sessions found");

            // 2️⃣ Calculate total amount (server-side!)
            var totalAmount = openSessions.Sum(s => s.Cost);

            // 3️⃣ Create payment (authoritative values)
            var licensePlates = string.Join(", ", openSessions.Select(s => s.LicensePlate));

            var newPayment = new Payment
            {
                Amount = totalAmount,
                Initiator = payment.Initiator,
                CreatedAt = DateTime.UtcNow,
                Completed = DateTime.UtcNow,
                Transaction = Guid.NewGuid().ToString("N"),
                Hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                TData = new TData
                {
                    Amount = totalAmount,
                    Date = DateTime.UtcNow,
                    Method = payment.TData.Method,
                    Issuer = licensePlates,
                    Bank = "internal"
                }
            };

            _context.Payments.Add(newPayment);

            //sessions as completed
            foreach (var session in openSessions)
            {
                session.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();
            return newPayment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}