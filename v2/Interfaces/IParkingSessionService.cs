public interface IParkingSessionService
{
    Task<IEnumerable<ParkingSession>> GetAllAsync();
    Task<ParkingSession?> GetByIdAsync(int id);
    Task<IEnumerable<ParkingSession>> GetByUserAsync(string username);
    Task<ParkingSession> StartSessionAsync(ParkingSession session);
    Task<ParkingSession> StopSessionAsync(int id);
    Task<bool> DeleteAsync(int id);
}