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

        //pay all
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

            //total amount 
            var totalAmount = openSessions.Sum(s => s.Cost);

            //create payment 
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
                    Bank = "ING"
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

        public async Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync(string licensePlate)
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .Where(s => s.LicensePlate == licensePlate && s.PaymentStatus == "Pending" && s.Stopped != default)
                .ToListAsync();
        }

        public async Task<Payment> PaySingleSessionAsync(string licensePlate, int sessionId, string method)
        {
            var session = await _context.ParkingSessions
                .FirstOrDefaultAsync(s =>
                    s.Id == sessionId &&
                    s.LicensePlate == licensePlate &&
                    s.PaymentStatus == "Pending" &&
                    s.Stopped != default);

            if (session == null)
                throw new InvalidOperationException("No unpaid session found for this license plate and session ID");

            var newPayment = new Payment
            {
                Amount = session.Cost,
                Initiator = session.Username,
                CreatedAt = DateTime.UtcNow,
                Completed = DateTime.UtcNow,
                Transaction = Guid.NewGuid().ToString("N"),
                Hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                TData = new TData
                {
                    Amount = session.Cost,
                    Date = DateTime.UtcNow,
                    Method = method,
                    Issuer = session.LicensePlate,
                    Bank = "IDeal"
                }
            };

            _context.Payments.Add(newPayment);

            session.PaymentStatus = "Completed";

            await _context.SaveChangesAsync();
            return newPayment;
        }
    }
}
