using v2.Models;

namespace v2.Services
{
    public interface IParkingSessionService
    {
        Task<ParkingSession> StartSessionAsync(
            int parkingLotId,
            string licensePlate,
            string username);

        Task<ParkingSession> StopSessionAsync(int sessionId);

        Task<ParkingSession?> GetByIdAsync(int sessionId);

        Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync();
    }
}