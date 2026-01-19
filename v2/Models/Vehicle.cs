using Newtonsoft.Json;
using v2.Validation;
using System.ComponentModel.DataAnnotations;

public class Vehicle
{
    public int Id { get; set; }

    [JsonProperty("user_id")]
    public int UserId { get; set; }

    [JsonProperty("license_plate")]
    public string LicensePlate { get; set; } = null!;

    [JsonProperty("make")]
    public string Make { get; set; } = null!;

    [JsonProperty("model")]
    public string Model { get; set; } = null!;

    [JsonProperty("color")]
    public string Color { get; set; } = null!;

    [JsonProperty("year")]
    public int Year { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class VehicleCreateDto
{
    [Required]
    [DutchLicensePlate]
    [JsonProperty("license_plate")]
    public string LicensePlate { get; set; } = null!;

    [JsonProperty("make")]
    public string Make { get; set; } = null!;

    [JsonProperty("model")]
    public string Model { get; set; } = null!;

    [JsonProperty("color")]
    public string Color { get; set; } = null!;

    [JsonProperty("year")]
    public int Year { get; set; }
}
