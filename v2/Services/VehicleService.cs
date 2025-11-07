using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace v2.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        //all vehicles
        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Take(50)
                .ToListAsync();

        }

        //vehicle by id
        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        //by user id
        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(int userId)
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        //new vehicle
        public async Task<Vehicle> CreateAsync(Vehicle vehicle)
        {
            vehicle.CreatedAt = DateTime.UtcNow;

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return vehicle;
        }

        //update vehicle
        public async Task<Vehicle> UpdateAsync(int id, Vehicle updated)
        {
            var existing = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (existing == null)
                throw new KeyNotFoundException("Vehicle not found.");

            existing.LicensePlate = updated.LicensePlate;
            existing.Make = updated.Make;
            existing.Model = updated.Model;
            existing.Color = updated.Color;
            existing.Year = updated.Year;

            _context.Vehicles.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        //delete vehicle
        public async Task<bool> DeleteAsync(int id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null) return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}