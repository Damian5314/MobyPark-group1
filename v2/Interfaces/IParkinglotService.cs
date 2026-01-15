using v2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace v2.Services
{
    public interface IParkingLotService
    {
        Task<IEnumerable<ParkingLot>> GetAllAsync();
        Task<ParkingLot?> GetByIdAsync(int id);
        Task<ParkingLot> CreateAsync(ParkingLot lot);
        Task<ParkingLot?> UpdateAsync(int id, ParkingLot lot);
        Task DeleteAsync(int id);
    }
}