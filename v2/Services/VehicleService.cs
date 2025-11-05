using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace v2.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly List<Vehicle> _vehicles = new();

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return _vehicles;
        }

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return _vehicles.FirstOrDefault(v => v.Id == id);
        }

        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(int userId)
        {
            return _vehicles.Where(v => v.UserId == userId);
        }

        public async Task<Vehicle> CreateAsync(Vehicle vehicle)
        {
            vehicle.Id = _vehicles.Count + 1;
            vehicle.CreatedAt = DateTime.UtcNow;
            _vehicles.Add(vehicle);
            return vehicle;
        }

        public async Task<Vehicle> UpdateAsync(int id, Vehicle updated)
        {
            var existing = _vehicles.FirstOrDefault(v => v.Id == id);
            if (existing == null)
                throw new KeyNotFoundException("Vehicle not found.");

            existing.LicensePlate = updated.LicensePlate;
            existing.Make = updated.Make;
            existing.Model = updated.Model;
            existing.Color = updated.Color;
            existing.Year = updated.Year;

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.Id == id);
            if (vehicle == null) return false;

            _vehicles.Remove(vehicle);
            return true;
        }
    }
}