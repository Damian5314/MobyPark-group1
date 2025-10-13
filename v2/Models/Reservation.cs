using System;

namespace v2.Models
{
    public class Reservation
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public int ParkingLotId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public decimal Cost { get; set; }
    }
}