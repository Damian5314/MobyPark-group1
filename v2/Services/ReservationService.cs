using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reservation> CreateAsync(ReservationCreateDto dto)
    {
        var lot = await _context.ParkingLots.FirstOrDefaultAsync(l => l.Id == dto.ParkingLotId);
        if (lot == null)
            throw new InvalidOperationException("Parking lot not found");

        if (lot.Reserved >= lot.Capacity)
            throw new InvalidOperationException("Parking lot is full");

        var reservation = new Reservation
        {
            UserId = dto.UserId,
            ParkingLotId = dto.ParkingLotId,
            VehicleId = dto.VehicleId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            CreatedAt = DateTime.UtcNow,
            Status = "Active"
        };

        lot.Reserved++;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task<Reservation> UpdateAsync(int id, ReservationCreateDto dto)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        reservation.StartTime = dto.StartTime;
        reservation.EndTime = dto.EndTime;
        reservation.VehicleId = dto.VehicleId;

        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            return false;

        var lot = await _context.ParkingLots.FindAsync(reservation.ParkingLotId);
        if (lot != null && lot.Reserved > 0)
            lot.Reserved--;

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return true;
    }

    // Convert reservation into an actual ParkingSession
    public async Task<ParkingSession> StartSessionFromReservationAsync(
        int reservationId,
        string licensePlate,
        string username)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.Status == "Active");

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found or inactive");

        var now = DateTime.UtcNow;
        if (now < reservation.StartTime)
            throw new InvalidOperationException("Reservation has not started yet");

        if (now > reservation.EndTime)
            throw new InvalidOperationException("Reservation has already expired");

        var existingSession = await _context.ParkingSessions
            .AnyAsync(s => s.ReservationId == reservationId);

        if (existingSession)
            throw new InvalidOperationException("Session already started for this reservation");

        var session = new ParkingSession
        {
            ParkingLotId = reservation.ParkingLotId,
            ReservationId = reservation.Id,
            LicensePlate = licensePlate,
            Username = username,
            Started = DateTime.UtcNow,
            PaymentStatus = "Pending"
        };

        _context.ParkingSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }
}