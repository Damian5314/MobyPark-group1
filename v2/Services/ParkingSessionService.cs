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

        public ParkingSessionService(
            AppDbContext context,
            IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        public async Task<ParkingSession> StartSessionAsync(
            int parkingLotId,
            string licensePlate,
            string username)
        {
            var lot = await _context.ParkingLots
                .FirstOrDefaultAsync(l => l.Id == parkingLotId);

            if (lot == null)
                throw new InvalidOperationException("Parking lot not found");

            if (lot.Reserved >= lot.Capacity)
                throw new InvalidOperationException("Parking lot is full");

            var activeSessionExists = await _context.ParkingSessions.AnyAsync(s =>
                s.LicensePlate == licensePlate &&
                s.Stopped == default);

            if (activeSessionExists)
                throw new InvalidOperationException("Active session already exists");

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

            return session;
        }

        public async Task<ParkingSession> StopSessionAsync(int sessionId)
        {
            var session = await _context.ParkingSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                throw new InvalidOperationException("Session not found");

            if (session.Stopped != default)
                return session;

            var lot = await _context.ParkingLots
                .FirstAsync(l => l.Id == session.ParkingLotId);

            session.Stopped = DateTime.UtcNow;
            session.DurationMinutes =
                (int)(session.Stopped - session.Started).TotalMinutes;

            session.Cost = CalculateCost(session.DurationMinutes, lot);
            session.PaymentStatus = ParkingPaymentStatus.Pending;

            lot.Reserved--;

            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ParkingSession?> GetByIdAsync(int sessionId)
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync()
        {
            return await _context.ParkingSessions
                .AsNoTracking()
                .Where(s => s.Stopped == default)
                .ToListAsync();
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
            var lot = await _context.ParkingLots.FirstOrDefaultAsync(l => l.Id == parkingLotId);
            if (lot == null)
                throw new InvalidOperationException("Parking lot not found");

            if (lot.Reserved >= lot.Capacity)
                throw new InvalidOperationException("Parking lot is full");

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

            return session;
        }
    }
}