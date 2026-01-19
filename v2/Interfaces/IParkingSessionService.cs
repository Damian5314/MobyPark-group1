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
        Task<IEnumerable<ParkingSession>> GetActiveSessionsByUsernameAsync(string username);
        Task<ParkingSession> CreateFromReservationAsync(
            int parkingLotId,
            string licensePlate,
            string username,
            DateTime startTime,
            DateTime endTime);
    }
}