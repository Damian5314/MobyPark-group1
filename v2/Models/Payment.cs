using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace v2.Models
{
    public class TData
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; } = null!;

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = null!;

        [JsonPropertyName("bank")]
        public string Bank { get; set; } = null!;
    }

    public class Payment
    {
        public int Id { get; set; } // EF Core primary key

        [JsonPropertyName("transaction")]
        public string Transaction { get; set; } = null!;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("initiator")]
        public string Initiator { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("completed")]
        public DateTime Completed { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = null!;

        [NotMapped] 
        [JsonPropertyName("t_data")]
        public TData TData { get; set; } = new();

        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        [JsonPropertyName("parking_lot_id")]
        public int? ParkingLotId { get; set; }
    }
}
