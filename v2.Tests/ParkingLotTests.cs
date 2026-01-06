using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests.Unit
{
    public class ParkingLotServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ParkingLotService _service;

        public ParkingLotServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ParkingLotTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);
            _service = new ParkingLotService(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            for (int i = 1; i <= 150; i++)
            {
                _context.ParkingLots.Add(new ParkingLot
                {
                    Id = i,
                    Name = $"Lot {i}",
                    Location = "Amsterdam",
                    Address = $"Street {i}",
                    Capacity = 100,
                    Reserved = 0,
                    Tariff = 2.5m,
                    DayTariff = 20m,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Max_100_ParkingLots()
        {
            var result = await _service.GetAllAsync();

            result.Should().NotBeNull();
            result.Count().Should().BeLessThanOrEqualTo(100);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_ParkingLot_When_Exists()
        {
            var lot = await _service.GetByIdAsync(1);

            lot.Should().NotBeNull();
            lot!.Name.Should().Be("Lot 1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var lot = await _service.GetByIdAsync(9999);

            lot.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Create_ParkingLot()
        {
            var lot = new ParkingLot
            {
                Name = "New Lot",
                Location = "Rotterdam",
                Address = "Teststraat 1",
                Capacity = 50,
                Tariff = 3,
                DayTariff = 25
            };

            var created = await _service.CreateAsync(lot);

            created.Id.Should().BeGreaterThan(0);
            _context.ParkingLots.Count().Should().Be(151);
        }
    }
}