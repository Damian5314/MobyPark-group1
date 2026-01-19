using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly IParkingSessionService _parkingSessionService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(AppDbContext context, IParkingSessionService parkingSessionService, ILogger<ReservationService> logger)
    {
        _context = context;
        _parkingSessionService = parkingSessionService;
        _logger = logger;
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all reservations");
            var reservations = await _context.Reservations
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Take(100)
                .ToListAsync();
            _logger.LogInformation("Retrieved {ReservationCount} reservations", reservations.Count);
            return reservations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all reservations");
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching reservations for user ID: {UserId}", userId);
            var reservations = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(100)
                .ToListAsync();
            _logger.LogInformation("Retrieved {ReservationCount} reservations for user {UserId}", reservations.Count, userId);
            return reservations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reservations for user ID: {UserId}", userId);
            throw;
        }
    }

    // get reservation by id
    public async Task<Reservation?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching reservation with ID: {ReservationId}", id);
            var reservation = await _context.Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found", id);
            }

            return reservation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reservation with ID: {ReservationId}", id);
            throw;
        }
    }


    // reservation functie aanmaken
    public async Task<Reservation> CreateAsync(ReservationCreateDto dto)
    {
        try
        {
            _logger.LogInformation("Creating reservation for UserId: {UserId}, ParkingLotId: {ParkingLotId}",
                dto.UserId, dto.ParkingLotId);

            var lot = await _context.ParkingLots.FirstOrDefaultAsync(l => l.Id == dto.ParkingLotId);
            if (lot == null)
            {
                _logger.LogWarning("Parking lot with ID {ParkingLotId} not found for reservation", dto.ParkingLotId);
                throw new InvalidOperationException("Parking lot not found");
            }

            if (lot.Reserved >= lot.Capacity)
            {
                _logger.LogWarning("Parking lot {ParkingLotId} is full. Cannot create reservation", dto.ParkingLotId);
                throw new InvalidOperationException("Parking lot is full");
            }

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

            _logger.LogInformation("Reservation created successfully with ID: {ReservationId}", reservation.Id);

            return reservation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation for UserId: {UserId}", dto.UserId);
            throw;
        }
    }

    public async Task<Reservation> UpdateAsync(int id, ReservationCreateDto dto)
    {
        try
        {
            _logger.LogInformation("Updating reservation with ID: {ReservationId}, Status: {Status}", id, dto.Status);

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found for update", id);
                throw new InvalidOperationException("Reservation not found");
            }

            reservation.StartTime = dto.StartTime;
            reservation.EndTime = dto.EndTime;
            reservation.VehicleId = dto.VehicleId;
            reservation.Status = dto.Status;

            //confirmed = start a parking session
            if (dto.Status == "Confirmed")
            {
                _logger.LogInformation("Reservation {ReservationId} confirmed. Creating parking session", id);

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == reservation.VehicleId);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found for reservation {ReservationId}",
                        reservation.VehicleId, id);
                    throw new InvalidOperationException("Vehicle not found");
                }

                await _parkingSessionService.CreateFromReservationAsync(
                    reservation.ParkingLotId,
                    vehicle.LicensePlate,
                    reservation.UserId.ToString(),
                    reservation.StartTime,
                    reservation.EndTime
                );
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} updated successfully", id);

            return reservation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation with ID: {ReservationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete reservation with ID: {ReservationId}", id);

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                _logger.LogWarning("Attempted to delete non-existent reservation with ID: {ReservationId}", id);
                return false;
            }

            var lot = await _context.ParkingLots.FindAsync(reservation.ParkingLotId);
            if (lot != null && lot.Reserved > 0)
                lot.Reserved--;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Reservation {ReservationId} deleted successfully", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reservation with ID: {ReservationId}", id);
            throw;
        }
    }

}