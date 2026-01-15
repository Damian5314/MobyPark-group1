using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;

namespace v2.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly AppDbContext _context;

        public ParkingLotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParkingLot>> GetAllAsync()
        {
            return await _context.ParkingLots
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Take(100)
                .ToListAsync();
        }

        public async Task<ParkingLot?> GetByIdAsync(int id)
        {
            return await _context.ParkingLots.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ParkingLot> CreateAsync(ParkingLot lot)
        {
            lot.CreatedAt = DateTime.UtcNow;

            //flat coordinates
            if (lot.Coordinates != null)
            {
                lot.Latitude = lot.Coordinates.Lat;
                lot.Longitude = lot.Coordinates.Lng;
            }

            _context.ParkingLots.Add(lot);
            await _context.SaveChangesAsync();
            return lot;
        }

        public async Task<ParkingLot?> UpdateAsync(int id, ParkingLot lot)
        {
            var existing = await _context.ParkingLots.FindAsync(id);
            if (existing == null)
                return null;

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
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var lot = await _context.ParkingLots.FindAsync(id);

            if (lot == null)
                throw new InvalidOperationException($"Parking lot with id {id} was not found.");

            _context.ParkingLots.Remove(lot);
            await _context.SaveChangesAsync();
        }
    }
}