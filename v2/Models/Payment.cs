using Newtonsoft.Json;

public class Payment
{
    public int Id { get; set; } // EF Core primary key

    [JsonProperty("transaction")]
    public string Transaction { get; set; } = null!;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("initiator")]
    public string Initiator { get; set; } = null!;

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("completed")]
    public DateTime Completed { get; set; }

    [JsonProperty("hash")]
    public string Hash { get; set; } = null!;

    [JsonProperty("t_data")]
    public TData TData { get; set; } = new();

    // Optional, only if they exist in JSON
    [JsonProperty("session_id")]
    public string? SessionId { get; set; }

    [JsonProperty("parking_lot_id")]
    public int? ParkingLotId { get; set; }
}

public class TData
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("method")]
    public string Method { get; set; } = null!;

    [JsonProperty("issuer")]
    public string Issuer { get; set; } = null!;

    [JsonProperty("bank")]
    public string Bank { get; set; } = null!;
}
