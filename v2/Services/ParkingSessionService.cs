using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ParkingSessionService : IParkingSessionService
{
    private readonly List<ParkingSession> _sessions = new();

    public async Task<IEnumerable<ParkingSession>> GetAllAsync()
    {
        return _sessions;
    }

    public async Task<ParkingSession?> GetByIdAsync(int id)
    {
        return _sessions.FirstOrDefault(s => s.Id == id);
    }

    public async Task<IEnumerable<ParkingSession>> GetByUserAsync(string username)
    {
        return _sessions.Where(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ParkingSession> StartSessionAsync(ParkingSession session)
    {
        session.Id = _sessions.Count + 1;
        session.Started = DateTime.UtcNow;
        session.Stopped = DateTime.MinValue;
        session.DurationMinutes = 0;
        session.Cost = 0;
        session.PaymentStatus = "Unpaid";

        _sessions.Add(session);
        return session;
    }

    public async Task<ParkingSession> StopSessionAsync(int id)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == id);
        if (session == null)
            throw new KeyNotFoundException("Session not found.");

        if (session.Stopped != DateTime.MinValue)
            throw new InvalidOperationException("Session already stopped.");

        session.Stopped = DateTime.UtcNow;
        session.DurationMinutes = (int)(session.Stopped - session.Started).TotalMinutes;

        // Simple cost calculation (e.g., €0.05 per minute)
        session.Cost = (decimal)(session.DurationMinutes * 0.05);
        session.PaymentStatus = "Pending";

        return session;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var session = _sessions.FirstOrDefault(s => s.Id == id);
        if (session == null) return false;

        _sessions.Remove(session);
        return true;
    }
}