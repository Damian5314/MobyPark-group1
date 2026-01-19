using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ParkingLotService> _logger;

        public ParkingLotService(AppDbContext context, ILogger<ParkingLotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ParkingLot>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all parking lots");
                var lots = await _context.ParkingLots
                    .AsNoTracking()
                    .OrderBy(p => p.Id)
                    .Take(100)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {LotCount} parking lots", lots.Count);
                return lots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all parking lots");
                throw;
            }
        }

        // get parkinglot by id 






        // get parkinglot 
        public async Task<ParkingLot?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching parking lot with ID: {ParkingLotId}", id);
                var lot = await _context.ParkingLots.FirstOrDefaultAsync(p => p.Id == id);
                if (lot == null)
                {
                    _logger.LogWarning("Parking lot with ID {ParkingLotId} not found", id);
                }
                return lot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching parking lot with ID: {ParkingLotId}", id);
                throw;
            }
        }

        public async Task<ParkingLot> CreateAsync(ParkingLot lot)
        {
            try
            {
                _logger.LogInformation("Creating new parking lot: {ParkingLotName}", lot.Name);
                lot.CreatedAt = DateTime.UtcNow;

                //flat coordinates
                if (lot.Coordinates != null)
                {
                    lot.Latitude = lot.Coordinates.Lat;
                    lot.Longitude = lot.Coordinates.Lng;
                }

                _context.ParkingLots.Add(lot);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Parking lot created successfully with ID: {ParkingLotId}", lot.Id);
                return lot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parking lot: {ParkingLotName}", lot.Name);
                throw;
            }
        }

        public async Task<ParkingLot?> UpdateAsync(int id, ParkingLot lot)
        {
            try
            {
                _logger.LogInformation("Updating parking lot with ID: {ParkingLotId}", id);
                var existing = await _context.ParkingLots.FindAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Attempted to update non-existent parking lot with ID: {ParkingLotId}", id);
                    return null;
                }

                existing.Name = lot.Name;
                existing.Location = lot.Location;
                existing.Address = lot.Address;
                existing.Capacity = lot.Capacity;
                existing.Reserved = lot.Reserved;
                existing.Tariff = lot.Tariff;
                existing.DayTariff = lot.DayTariff;

                if (lot.Coordinates != null)
                {
                    existing.Latitude = lot.Coordinates.Lat;
                    existing.Longitude = lot.Coordinates.Lng;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Parking lot {ParkingLotId} updated successfully", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parking lot with ID: {ParkingLotId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete parking lot with ID: {ParkingLotId}", id);
                var lot = await _context.ParkingLots.FindAsync(id);
                if (lot == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent parking lot with ID: {ParkingLotId}", id);
                    return false;
                }

                _context.ParkingLots.Remove(lot);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Parking lot {ParkingLotId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parking lot with ID: {ParkingLotId}", id);
                throw;
            }
        }
    }
}