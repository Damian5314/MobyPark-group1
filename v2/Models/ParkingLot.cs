using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace v2.Models
{
    public class Coordinates
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }

    public class ParkingLot
    {
        public int? hotel_id { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("location")]
        public string Location { get; set; } = null!;

        [JsonProperty("address")]
        public string Address { get; set; } = null!;

        [JsonProperty("capacity")]
        public int Capacity { get; set; }

        [JsonProperty("reserved")]
        public int Reserved { get; set; }

        [JsonProperty("tariff")]
        public decimal Tariff { get; set; }

        [JsonProperty("daytariff")]
        public decimal DayTariff { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        // Flattened coordinates for DB storage
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Only used for JSON deserialization
        [JsonProperty("coordinates")]
        [NotMapped]
        public Coordinates Coordinates { get; set; } = new();
    }
}
