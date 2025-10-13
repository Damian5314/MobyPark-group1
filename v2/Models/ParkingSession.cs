using Newtonsoft.Json;

public class ParkingSession
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("parking_lot_id")]
    public int ParkingLotId { get; set; }

    [JsonProperty("licenseplate")]
    public string LicensePlate { get; set; } = null!;

    // JSON field "user", keep for mapping later
    [JsonProperty("user")]
    public string Username { get; set; } = null!;

    [JsonProperty("started")]
    public DateTime Started { get; set; }

    [JsonProperty("stopped")]
    public DateTime Stopped { get; set; }

    [JsonProperty("duration_minutes")]
    public int DurationMinutes { get; set; }

    [JsonProperty("cost")]
    public decimal Cost { get; set; }

    [JsonProperty("payment_status")]
    public string PaymentStatus { get; set; } = null!;
}
