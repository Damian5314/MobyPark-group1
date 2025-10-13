using Newtonsoft.Json;
using System;

namespace v2.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("parking_lot_id")]
        public int ParkingLotId { get; set; }

        [JsonProperty("vehicle_id")]
        public int VehicleId { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime EndTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = null!;

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("cost")]
        public decimal Cost { get; set; }
    }
}
