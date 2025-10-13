using System;

namespace v2.Models
{
    public class Coordinates
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class ParkingLot
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Capacity { get; set; }
        public int Reserved { get; set; }
        public decimal Tariff { get; set; }
        public decimal DayTariff { get; set; }
        public DateTime CreatedAt { get; set; }
        public Coordinates Coordinates { get; set; } = new();
    }

}