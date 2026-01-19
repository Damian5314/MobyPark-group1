public class ReservationCreateDto
{
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public int ParkingLotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Active"; //"Active", "Confirmed", "Cancelled"
}

public class UserReservationCreateDto
{
    public int VehicleId { get; set; }
    public int ParkingLotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class StartSessionFromReservationDto
{
    public string LicensePlate { get; set; } = null!;
    public string Username { get; set; } = null!;
}