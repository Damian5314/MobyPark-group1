using System;

namespace v2.Models
{
    public class Vehicle
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Color { get; set; } = null!;
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
