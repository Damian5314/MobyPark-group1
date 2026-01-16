using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
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
            var loggerMock = new Mock<ILogger<ParkingLotService>>();
            _service = new ParkingLotService(_context, loggerMock.Object);

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
                    Name = $"Lot {i}",
                    Location = "Amsterdam",
                    Address = $"Street {i}",
                    Capacity = 100,
                    Reserved = 0,
                    Tariff = 2.5m,
                    DayTariff = 20m,
                    CreatedAt = DateTime.UtcNow
                    // DO NOT set Id manually
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
            var firstLot = await _context.ParkingLots.FirstAsync();
            var lot = await _service.GetByIdAsync(firstLot.Id);

            lot.Should().NotBeNull();
            lot!.Name.Should().Be(firstLot.Name);
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

        [Fact]
        public async Task UpdateAsync_Should_Modify_Existing_ParkingLot()
        {
            var lot = await _context.ParkingLots.FirstAsync();
            lot.Name = "Updated Lot";
            lot.Capacity = 200;

            var updated = await _service.UpdateAsync(lot.Id, lot);

            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Lot");
            updated.Capacity.Should().Be(200);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotFound()
        {
            var lot = new ParkingLot
            {
                Name = "NonExistent",
                Location = "Rotterdam",
                Address = "Nowhere",
                Capacity = 50,
                Tariff = 3,
                DayTariff = 25
            };

            var updated = await _service.UpdateAsync(9999, lot);
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_ParkingLot_When_Exists()
        {
            var initialCount = _context.ParkingLots.Count();
            var lot = await _context.ParkingLots.FirstAsync();

            var deleted = await _service.DeleteAsync(lot.Id);

            deleted.Should().BeTrue();
            _context.ParkingLots.Count().Should().Be(initialCount - 1);
            _context.ParkingLots.Find(lot.Id).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            var deleted = await _service.DeleteAsync(9999999);
            deleted.Should().BeFalse();
        }
    }
}