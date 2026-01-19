using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly IParkingSessionService _parkingSessionService;

    public ReservationService(AppDbContext context, IParkingSessionService parkingSessionService)
    {
        _context = context;
        _parkingSessionService = parkingSessionService;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    // get reservation by id
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
        reservation.Status = dto.Status;

        //confirmed = start a parking session
        if (dto.Status == "Confirmed")
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == reservation.VehicleId);
            if (vehicle == null)
                throw new InvalidOperationException("Vehicle not found");

            await _parkingSessionService.CreateFromReservationAsync(
                reservation.ParkingLotId,
                vehicle.LicensePlate,
                reservation.UserId.ToString(),
                reservation.StartTime,
                reservation.EndTime
            );
        }

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

}