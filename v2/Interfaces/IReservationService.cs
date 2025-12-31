using v2.Models;

public interface IReservationService
{
    Task<IEnumerable<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation> CreateAsync(ReservationCreateDto dto);
    Task<Reservation> UpdateAsync(int id, ReservationCreateDto dto);
    Task<bool> DeleteAsync(int id);

    Task<ParkingSession> StartSessionFromReservationAsync(int reservationId, string licensePlate, string username);
}