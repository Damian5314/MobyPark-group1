public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator);
    Task<Payment> CreateAsync(PaymentCreateDto dto);
    Task<Payment> PaySingleSessionAsync(PaySingleSessionDto dto);
    Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync(string licensePlate);
    Task<bool> DeleteAsync(int id);
}