using v2.Models;

namespace v2.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly List<ParkingLot> _lots = new();
        private readonly List<ParkingSession> _sessions = new();

        public async Task<IEnumerable<ParkingLot>> GetAllAsync()
        {
            return _lots;
        }

        public async Task<ParkingLot?> GetByIdAsync(int id)
        {
            return _lots.FirstOrDefault(p => p.Id == id);
        }

        public async Task<ParkingLot> CreateAsync(ParkingLot lot)
        {
            lot.Id = _lots.Count + 1;
            lot.CreatedAt = DateTime.UtcNow;
            _lots.Add(lot);
            return lot;
        }

        public async Task<ParkingLot> UpdateAsync(int id, ParkingLot updated)
        {
            var existing = _lots.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                throw new KeyNotFoundException("Parking lot not found.");

            existing.Name = updated.Name;
            existing.Location = updated.Location;
            existing.Address = updated.Address;
            existing.Capacity = updated.Capacity;
            existing.Tariff = updated.Tariff;
            existing.DayTariff = updated.DayTariff;
            existing.Latitude = updated.Latitude;
            existing.Longitude = updated.Longitude;

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var lot = _lots.FirstOrDefault(p => p.Id == id);
            if (lot == null) return false;
            _lots.Remove(lot);
            return true;
        }

        // -----------------------
        // Parking Session Actions
        // -----------------------

        public async Task<ParkingSession> StartSessionAsync(int parkingLotId, string licensePlate, string username)
        {
            var lot = _lots.FirstOrDefault(p => p.Id == parkingLotId);
            if (lot == null)
                throw new KeyNotFoundException("Parking lot not found.");

            var active = _sessions.FirstOrDefault(s => s.LicensePlate == licensePlate && s.Stopped == default);
            if (active != null)
                throw new InvalidOperationException("Session already active for this vehicle.");

            var session = new ParkingSession
            {
                Id = _sessions.Count + 1,
                ParkingLotId = parkingLotId,
                LicensePlate = licensePlate,
                Username = username,
                Started = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };

            _sessions.Add(session);
            lot.Reserved++;
            return session;
        }

        public async Task<ParkingSession> StopSessionAsync(int parkingLotId, string licensePlate, string username)
        {
            var session = _sessions.FirstOrDefault(s =>
                s.ParkingLotId == parkingLotId &&
                s.LicensePlate == licensePlate &&
                s.Stopped == default);

            if (session == null)
                throw new InvalidOperationException("No active session found.");

            session.Stopped = DateTime.UtcNow;
            session.DurationMinutes = (int)(session.Stopped - session.Started).TotalMinutes;
            session.Cost = session.DurationMinutes * 0.05m; // simple cost example
            session.PaymentStatus = "Completed";

            var lot = _lots.FirstOrDefault(p => p.Id == parkingLotId);
            if (lot != null && lot.Reserved > 0)
                lot.Reserved--;

            return session;
        }

        public async Task<IEnumerable<ParkingSession>> GetSessionsAsync(int parkingLotId)
        {
            return _sessions.Where(s => s.ParkingLotId == parkingLotId);
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return false;
            _sessions.Remove(session);
            return true;
        }
    }
}