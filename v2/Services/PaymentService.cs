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
        public async Task<Payment> CreateAsync(PaymentCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Initiator))
                throw new InvalidOperationException("Initiator is required");

            if (string.IsNullOrWhiteSpace(dto.Method))
                throw new InvalidOperationException("Payment method is required");

            if (string.IsNullOrWhiteSpace(dto.LicensePlate))
                throw new InvalidOperationException("License plate is required");

            var openSessions = await _context.ParkingSessions
                .Where(s => s.PaymentStatus == "Pending" &&
                            s.Stopped != default &&
                            s.LicensePlate == dto.LicensePlate)
                .ToListAsync();

            if (!openSessions.Any())
                throw new InvalidOperationException($"No unpaid parking sessions found for license plate {dto.LicensePlate}");

            var totalAmount = openSessions.Sum(s => s.Cost);

            var payment = new Payment
            {
                Amount = totalAmount,
                Initiator = dto.Initiator,
                CreatedAt = DateTime.UtcNow,
                Completed = DateTime.UtcNow,
                Transaction = Guid.NewGuid().ToString("N"),
                Hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                SessionId = string.Join(",", openSessions.Select(s => s.Id)),
                ParkingLotId = openSessions.FirstOrDefault()?.ParkingLotId, // optional: first lot ID
                TData = new TData
                {
                    Amount = totalAmount,
                    Date = DateTime.UtcNow,
                    Method = dto.Method,
                    Issuer = dto.Initiator,
                    Bank = dto.Bank
                }
            };

            _context.Payments.Add(payment);

            foreach (var session in openSessions)
            {
                session.Username = dto.Initiator;
                session.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();
            return payment;
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

        public async Task<Payment> PaySingleSessionAsync(PaySingleSessionDto dto)
        {
            var session = await _context.ParkingSessions
                .FirstOrDefaultAsync(s =>
                    s.Id == dto.SessionId &&
                    s.LicensePlate == dto.LicensePlate &&
                    s.PaymentStatus == "Pending" &&
                    s.Stopped != default);

            if (session == null)
                throw new InvalidOperationException("No unpaid session found for this license plate and session ID");

            var newPayment = new Payment
            {
                Amount = session.Cost,
                Initiator = dto.Initiator,
                CreatedAt = DateTime.UtcNow,
                Completed = DateTime.UtcNow,
                Transaction = Guid.NewGuid().ToString("N"),
                Hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                SessionId = session.Id.ToString(),
                ParkingLotId = session.ParkingLotId,
                TData = new TData
                {
                    Amount = session.Cost,
                    Date = DateTime.UtcNow,
                    Method = dto.Method,
                    Issuer = dto.Initiator,
                    Bank = dto.Bank
                }
            };

            _context.Payments.Add(newPayment);

            session.Username = dto.Initiator;
            session.PaymentStatus = "Completed";

            await _context.SaveChangesAsync();
            return newPayment;
        }
    }
}
