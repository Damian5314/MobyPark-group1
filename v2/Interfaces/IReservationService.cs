using v2.Models;

namespace v2.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<Reservation> CreateAsync(Reservation reservation);
        Task<Reservation> UpdateAsync(int id, Reservation updated);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
    }
}