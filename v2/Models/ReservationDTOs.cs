public class ReservationCreateDto
{
    public int UserId { get; set; }
    public int ParkingLotId { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class StartSessionFromReservationDto
{
    public string LicensePlate { get; set; } = null!;
    public string Username { get; set; } = null!;
}