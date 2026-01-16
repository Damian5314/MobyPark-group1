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
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(AppDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //all vehicles
        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all vehicles");
                var vehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Take(50)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {VehicleCount} vehicles", vehicles.Count);
                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all vehicles");
                throw;
            }
        }

        //vehicle by id
        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching vehicle with ID: {VehicleId}", id);
                var vehicle = await _context.Vehicles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", id);
                }

                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle with ID: {VehicleId}", id);
                throw;
            }
        }

        //by user id
        public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching vehicles for user ID: {UserId}", userId);
                var vehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.UserId == userId)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {VehicleCount} vehicles for user {UserId}", vehicles.Count, userId);
                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles for user ID: {UserId}", userId);
                throw;
            }
        }

        //new vehicle
        public async Task<Vehicle> CreateAsync(Vehicle vehicle)
        {
            try
            {
                _logger.LogInformation("Creating new vehicle for user {UserId}: {LicensePlate}", vehicle.UserId, vehicle.LicensePlate);
                vehicle.CreatedAt = DateTime.UtcNow;

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Vehicle created successfully with ID: {VehicleId}", vehicle.Id);
                return vehicle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle for user {UserId}", vehicle.UserId);
                throw;
            }
        }

        //update vehicle
        public async Task<Vehicle> UpdateAsync(int id, Vehicle updated)
        {
            try
            {
                _logger.LogInformation("Updating vehicle with ID: {VehicleId}", id);
                var existing = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
                if (existing == null)
                {
                    _logger.LogWarning("Attempted to update non-existent vehicle with ID: {VehicleId}", id);
                    throw new KeyNotFoundException("Vehicle not found.");
                }

                existing.LicensePlate = updated.LicensePlate;
                existing.Make = updated.Make;
                existing.Model = updated.Model;
                existing.Color = updated.Color;
                existing.Year = updated.Year;

                _context.Vehicles.Update(existing);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Vehicle {VehicleId} updated successfully", id);
                return existing;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle with ID: {VehicleId}", id);
                throw;
            }
        }

        //delete vehicle
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete vehicle with ID: {VehicleId}", id);
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
                if (vehicle == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent vehicle with ID: {VehicleId}", id);
                    return false;
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Vehicle {VehicleId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID: {VehicleId}", id);
                throw;
            }
        }
    }
}