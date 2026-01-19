using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all payments");
                var payments = await _context.Payments
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(100)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {PaymentCount} payments", payments.Count);
                return payments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all payments");
                throw;
            }
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching payment with ID: {PaymentId}", id);
                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", id);
                }

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment with ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator)
        {
            try
            {
                _logger.LogInformation("Fetching payments for initiator: {Initiator}", initiator);
                var payments = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.Initiator == initiator)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(100)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {PaymentCount} payments for initiator: {Initiator}", payments.Count, initiator);
                return payments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payments for initiator: {Initiator}", initiator);
                throw;
            }
        }

        //pay all
        public async Task<Payment> CreateAsync(PaymentCreateDto dto)
        {
            try
            {
                _logger.LogInformation("Processing payment for license plate: {LicensePlate}, Initiator: {Initiator}",
                    dto.LicensePlate, dto.Initiator);

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
                {
                    _logger.LogWarning("No unpaid parking sessions found for license plate: {LicensePlate}", dto.LicensePlate);
                    throw new InvalidOperationException($"No unpaid parking sessions found for license plate {dto.LicensePlate}");
                }

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

                _logger.LogInformation("Payment created successfully. Transaction: {Transaction}, Amount: {Amount}, Sessions: {SessionCount}",
                    payment.Transaction, totalAmount, openSessions.Count);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for license plate: {LicensePlate}", dto.LicensePlate);
                throw;
            }
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete payment with ID: {PaymentId}", id);
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found for deletion", id);
                    return false;
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Payment with ID {PaymentId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync(string licensePlate)
        {
            try
            {
                _logger.LogInformation("Fetching unpaid sessions for license plate: {LicensePlate}", licensePlate);
                var sessions = await _context.ParkingSessions
                    .AsNoTracking()
                    .Where(s => s.LicensePlate == licensePlate && s.PaymentStatus == "Pending" && s.Stopped != default)
                    .ToListAsync();
                _logger.LogInformation("Found {SessionCount} unpaid sessions for license plate: {LicensePlate}",
                    sessions.Count, licensePlate);
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unpaid sessions for license plate: {LicensePlate}", licensePlate);
                throw;
            }
        }

        public async Task<Payment> PaySingleSessionAsync(PaySingleSessionDto dto)
        {
            try
            {
                _logger.LogInformation("Processing single session payment for SessionId: {SessionId}, LicensePlate: {LicensePlate}",
                    dto.SessionId, dto.LicensePlate);

                var session = await _context.ParkingSessions
                    .FirstOrDefaultAsync(s =>
                        s.Id == dto.SessionId &&
                        s.LicensePlate == dto.LicensePlate &&
                        s.PaymentStatus == "Pending" &&
                        s.Stopped != default);

                if (session == null)
                {
                    _logger.LogWarning("No unpaid session found for SessionId: {SessionId}, LicensePlate: {LicensePlate}",
                        dto.SessionId, dto.LicensePlate);
                    throw new InvalidOperationException("No unpaid session found for this license plate and session ID");
                }

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

                _logger.LogInformation("Single session payment completed. Transaction: {Transaction}, Amount: {Amount}",
                    newPayment.Transaction, session.Cost);

                return newPayment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing single session payment for SessionId: {SessionId}", dto.SessionId);
                throw;
            }
        }
    }
}
