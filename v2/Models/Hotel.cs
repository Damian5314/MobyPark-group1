using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace v2.Models
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //parking lots for this hotel
        public List<ParkingLot> ParkingLots { get; set; } = new();
    }
}