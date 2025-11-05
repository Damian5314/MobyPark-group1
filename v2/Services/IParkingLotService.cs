using v2.Models;

namespace v2.Services
{
    public interface IParkingLotService
    {
        Task<IEnumerable<ParkingLot>> GetAllAsync();
        Task<ParkingLot?> GetByIdAsync(int id);
        Task<ParkingLot> CreateAsync(ParkingLot lot);
        Task<ParkingLot> UpdateAsync(int id, ParkingLot updated);
        Task<bool> DeleteAsync(int id);

        // Parking session actions
        Task<ParkingSession> StartSessionAsync(int parkingLotId, string licensePlate, string username);
        Task<ParkingSession> StopSessionAsync(int parkingLotId, string licensePlate, string username);
        Task<IEnumerable<ParkingSession>> GetSessionsAsync(int parkingLotId);
        Task<bool> DeleteSessionAsync(int sessionId);
    }
}