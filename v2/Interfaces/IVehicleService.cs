using System.Collections.Generic;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<Vehicle?> GetByIdAsync(int id);
        Task<IEnumerable<Vehicle>> GetByUserIdAsync(int userId);
        Task<Vehicle> CreateAsync(Vehicle vehicle);
        Task<Vehicle> UpdateAsync(int id, Vehicle updated);
        Task<bool> DeleteAsync(int id);
    }
}