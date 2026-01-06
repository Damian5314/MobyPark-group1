using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using v2.Data;
using v2.Models;
using v2.Services;
using Xunit;
using FluentAssertions;

namespace v2.Tests
{
    public class ReservationServiceTests
    {
        private ReservationService _service;
        private AppDbContext _context;
        private Mock<IParkingSessionService> _parkingSessionMock;

        public ReservationServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _parkingSessionMock = new Mock<IParkingSessionService>();

            SeedDatabase(_context);

            _service = new ReservationService(_context, _parkingSessionMock.Object);
        }

        private void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed Parking Lots
            context.ParkingLots.AddRange(
                new ParkingLot
                {
                    Id = 1,
                    Name = "Lot A",
                    Capacity = 2,
                    Reserved = 0,
                    Address = "123 Main St",
                    Location = "City Center",
                    Tariff = 5,
                    DayTariff = 20
                }
            );

            // Seed Vehicles
            context.Vehicles.AddRange(
                new Vehicle
                {
                    Id = 1,
                    UserId = 1,
                    LicensePlate = "ABC123",
                    Make = "Toyota",
                    Model = "Corolla",
                    Color = "Blue",
                    Year = 2020,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Reservations
            context.Reservations.AddRange(
                new Reservation
                {
                    Id = 1,
                    UserId = 1,
                    ParkingLotId = 1,
                    VehicleId = 1,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    Cost = 0
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task Create_Should_Create_Reservation()
        {
            var dto = new ReservationCreateDto
            {
                ParkingLotId = 1,
                UserId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = "Active"
            };

            var reservation = await _service.CreateAsync(dto);

            reservation.Should().NotBeNull();
            reservation.Status.Should().Be("Active");

            var lot = await _context.ParkingLots.FindAsync(1);
            lot.Reserved.Should().Be(1);
        }

        [Fact]
        public async Task Update_To_Confirmed_Should_Create_ParkingSession()
        {
            var reservation = await _service.CreateAsync(new ReservationCreateDto
            {
                ParkingLotId = 1,
                UserId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            });

            _parkingSessionMock.Setup(p => p.CreateFromReservationAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()
            )).ReturnsAsync(new ParkingSession { Id = 1 });

            var updated = await _service.UpdateAsync(reservation.Id, new ReservationCreateDto
            {
                ParkingLotId = 1,
                UserId = 1,
                VehicleId = 1,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = "Confirmed"
            });

            updated.Status.Should().Be("Confirmed");
            _parkingSessionMock.Verify(p => p.CreateFromReservationAsync(
                1, "ABC123", "1", reservation.StartTime, reservation.EndTime), Times.Once);
        }

        [Fact]
        public async Task Delete_Should_Remove_Reservation()
        {
            var reservation = await _service.CreateAsync(new ReservationCreateDto
            {
                ParkingLotId = 1,
                UserId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            });

            var result = await _service.DeleteAsync(reservation.Id);
            result.Should().BeTrue();

            var dbReservation = await _context.Reservations.FindAsync(reservation.Id);
            dbReservation.Should().BeNull();

            var lot = await _context.ParkingLots.FindAsync(1);
            lot.Reserved.Should().Be(0);
        }
    }
}