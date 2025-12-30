public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator);
    Task<Payment> CreateAsync(Payment payment); //pay all
    Task<Payment> PaySingleSessionAsync(string licensePlate, int sessionId, string method);
    Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync(string licensePlate);
    Task<bool> DeleteAsync(int id);
}