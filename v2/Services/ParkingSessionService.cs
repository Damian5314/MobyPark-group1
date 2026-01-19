using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class ParkingSessionService : IParkingSessionService
    {
        private readonly AppDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ParkingSessionService> _logger;

        public ParkingSessionService(
            AppDbContext context,
            IPaymentService paymentService,
            ILogger<ParkingSessionService> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<ParkingSession> StartSessionAsync(
            int parkingLotId,
            string licensePlate,
            string username)
        {
            try
            {
                _logger.LogInformation("Starting parking session for LicensePlate: {LicensePlate}, ParkingLot: {ParkingLotId}",
                    licensePlate, parkingLotId);

                var lot = await _context.ParkingLots
                    .FirstOrDefaultAsync(l => l.Id == parkingLotId);

                if (lot == null)
                {
                    _logger.LogWarning("Parking lot with ID {ParkingLotId} not found", parkingLotId);
                    throw new InvalidOperationException("Parking lot not found");
                }

                if (lot.Reserved >= lot.Capacity)
                {
                    _logger.LogWarning("Parking lot {ParkingLotId} is full. Reserved: {Reserved}, Capacity: {Capacity}",
                        parkingLotId, lot.Reserved, lot.Capacity);
                    throw new InvalidOperationException("Parking lot is full");
                }

                var activeSessionExists = await _context.ParkingSessions.AnyAsync(s =>
                    s.LicensePlate == licensePlate &&
                    s.Stopped == default);

                if (activeSessionExists)
                {
                    _logger.LogWarning("Active session already exists for license plate: {LicensePlate}", licensePlate);
                    throw new InvalidOperationException("Active session already exists");
                }

                var session = new ParkingSession
                {
                    ParkingLotId = parkingLotId,
                    LicensePlate = licensePlate,
                    Username = username,
                    Started = DateTime.UtcNow,
                    PaymentStatus = "Pending"
                };

                lot.Reserved++;

                _context.ParkingSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parking session started successfully. SessionId: {SessionId}, LicensePlate: {LicensePlate}",
                    session.Id, licensePlate);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting parking session for LicensePlate: {LicensePlate}", licensePlate);
                throw;
            }
        }

        public async Task<ParkingSession> StopSessionAsync(int sessionId)
        {
            try
            {
                _logger.LogInformation("Stopping parking session with ID: {SessionId}", sessionId);

                var session = await _context.ParkingSessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                {
                    _logger.LogWarning("Session with ID {SessionId} not found", sessionId);
                    throw new InvalidOperationException("Session not found");
                }

                if (session.Stopped != default)
                {
                    _logger.LogInformation("Session {SessionId} already stopped", sessionId);
                    return session;
                }

                var lot = await _context.ParkingLots
                    .FirstAsync(l => l.Id == session.ParkingLotId);

                session.Stopped = DateTime.UtcNow;
                session.DurationMinutes =
                    (int)(session.Stopped - session.Started).TotalMinutes;

                session.Cost = CalculateCost(session.DurationMinutes, lot);
                session.PaymentStatus = ParkingPaymentStatus.Pending;

                lot.Reserved--;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Parking session stopped. SessionId: {SessionId}, Duration: {Duration} minutes, Cost: {Cost}",
                    sessionId, session.DurationMinutes, session.Cost);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping parking session with ID: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<ParkingSession?> GetByIdAsync(int sessionId)
        {
            try
            {
                _logger.LogInformation("Fetching parking session with ID: {SessionId}", sessionId);
                var session = await _context.ParkingSessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                {
                    _logger.LogWarning("Parking session with ID {SessionId} not found", sessionId);
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching parking session with ID: {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching active parking sessions");
                var sessions = await _context.ParkingSessions
                    .AsNoTracking()
                    .Where(s => s.Stopped == default)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {SessionCount} active parking sessions", sessions.Count);
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active parking sessions");
                throw;
            }
        }

        public async Task<IEnumerable<ParkingSession>> GetActiveSessionsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching active parking sessions for user ID: {UserId}", userId);

                // Get all license plates belonging to this user
                var userLicensePlates = await _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.UserId == userId)
                    .Select(v => v.LicensePlate)
                    .ToListAsync();

                // Get active sessions for those license plates
                var sessions = await _context.ParkingSessions
                    .AsNoTracking()
                    .Where(s => s.Stopped == default && userLicensePlates.Contains(s.LicensePlate))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {SessionCount} active parking sessions for user ID: {UserId}", sessions.Count, userId);
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active parking sessions for user ID: {UserId}", userId);
                throw;
            }
        }

        private static decimal CalculateCost(int minutes, ParkingLot lot)
        {
            if (minutes <= 60)
                return lot.Tariff;

            var hours = Math.Ceiling(minutes / 60m);
            return hours * lot.DayTariff;
        }

        public async Task<ParkingSession> CreateFromReservationAsync(
        int parkingLotId,
        string licensePlate,
        string username,
        DateTime startTime,
        DateTime endTime)
        {
            try
            {
                _logger.LogInformation("Creating parking session from reservation. ParkingLot: {ParkingLotId}, LicensePlate: {LicensePlate}",
                    parkingLotId, licensePlate);

                var lot = await _context.ParkingLots.FirstOrDefaultAsync(l => l.Id == parkingLotId);
                if (lot == null)
                {
                    _logger.LogWarning("Parking lot with ID {ParkingLotId} not found for reservation", parkingLotId);
                    throw new InvalidOperationException("Parking lot not found");
                }

                if (lot.Reserved >= lot.Capacity)
                {
                    _logger.LogWarning("Parking lot {ParkingLotId} is full for reservation. Reserved: {Reserved}, Capacity: {Capacity}",
                        parkingLotId, lot.Reserved, lot.Capacity);
                    throw new InvalidOperationException("Parking lot is full");
                }

                var session = new ParkingSession
                {
                    ParkingLotId = parkingLotId,
                    LicensePlate = licensePlate,
                    Username = username,
                    Started = startTime,
                    Stopped = endTime,
                    DurationMinutes = (int)(endTime - startTime).TotalMinutes,
                    Cost = CalculateCost((int)(endTime - startTime).TotalMinutes, lot),
                    PaymentStatus = "Pending"
                };

                lot.Reserved++;
                _context.ParkingSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parking session created from reservation. SessionId: {SessionId}, Duration: {Duration} minutes",
                    session.Id, session.DurationMinutes);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parking session from reservation for ParkingLot: {ParkingLotId}", parkingLotId);
                throw;
            }
        }
    }
}