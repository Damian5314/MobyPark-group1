using v2.Models;

namespace v2.Services
{
    public class ReservationService : IReservationService
    {
        private readonly List<Reservation> _reservations = new();

        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return _reservations;
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return _reservations.FirstOrDefault(r => r.Id == id);
        }

        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            reservation.Id = _reservations.Count + 1;
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.Status = "Active";
            _reservations.Add(reservation);
            return reservation;
        }

        public async Task<Reservation> UpdateAsync(int id, Reservation updated)
        {
            var existing = _reservations.FirstOrDefault(r => r.Id == id);
            if (existing == null)
                throw new KeyNotFoundException("Reservation not found.");

            existing.StartTime = updated.StartTime;
            existing.EndTime = updated.EndTime;
            existing.Status = updated.Status;
            existing.Cost = updated.Cost;
            existing.VehicleId = updated.VehicleId;
            existing.ParkingLotId = updated.ParkingLotId;

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == id);
            if (reservation == null) return false;

            _reservations.Remove(reservation);
            return true;
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return _reservations.Where(r => r.UserId == userId);
        }
    }
}