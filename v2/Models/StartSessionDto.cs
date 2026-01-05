using Microsoft.Extensions.Diagnostics.HealthChecks;

public class StartSessionDto
{
    public int ParkingLotId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Username { get; set; } = null!;

}