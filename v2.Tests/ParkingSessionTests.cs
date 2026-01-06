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
    public class ParkingSessionServiceTests
    {
        private ParkingSessionService _service;
        private AppDbContext _context;

        public ParkingSessionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            _context = new AppDbContext(options);
            SeedDatabase(_context);

            var paymentMock = new Mock<IPaymentService>();
            _service = new ParkingSessionService(_context, paymentMock.Object);
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
                },
                new ParkingLot
                {
                    Id = 2,
                    Name = "Lot B",
                    Capacity = 3,
                    Reserved = 1,
                    Address = "456 Side St",
                    Location = "Uptown",
                    Tariff = 4,
                    DayTariff = 15
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
                },
                new Vehicle
                {
                    Id = 2,
                    UserId = 2,
                    LicensePlate = "XYZ789",
                    Make = "Honda",
                    Model = "Civic",
                    Color = "Red",
                    Year = 2021,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Optional: seed ParkingSessions if needed for tests
            context.ParkingSessions.AddRange(
                new ParkingSession
                {
                    Id = 1,
                    ParkingLotId = 2,
                    LicensePlate = "XYZ789",
                    Username = "user2",
                    Started = DateTime.UtcNow.AddHours(-1),
                    Stopped = default,
                    PaymentStatus = "Pending"
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task StartSession_Should_Create_New_Session()
        {
            var session = await _service.StartSessionAsync(1, "XYZ123", "user1");

            session.Should().NotBeNull();
            session.LicensePlate.Should().Be("XYZ123");

            var lot = await _context.ParkingLots.FindAsync(1);
            lot.Reserved.Should().Be(1);
        }

        [Fact]
        public async Task StartSession_Should_Fail_When_Lot_Is_Full()
        {
            // fill the lot
            _context.ParkingLots.Find(1)!.Reserved = 2;
            _context.SaveChanges();

            Func<Task> act = async () => await _service.StartSessionAsync(1, "XYZ123", "user1");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Parking lot is full");
        }

        [Fact]
        public async Task StopSession_Should_Set_Stopped_And_Cost()
        {
            var session = await _service.StartSessionAsync(1, "XYZ123", "user1");

            await Task.Delay(10); // simulate time passing
            var stoppedSession = await _service.StopSessionAsync(session.Id);

            stoppedSession.Stopped.Should().NotBe(default);
            stoppedSession.Cost.Should().BeGreaterThan(0);

            var lot = await _context.ParkingLots.FindAsync(1);
            lot.Reserved.Should().Be(0);
        }

        [Fact]
        public async Task CreateFromReservation_Should_Create_Completed_Session()
        {
            var start = DateTime.UtcNow.AddHours(-2);
            var end = DateTime.UtcNow;

            var session = await _service.CreateFromReservationAsync(
                1, "RES123", "user1", start, end);

            session.Should().NotBeNull();
            session.DurationMinutes.Should().Be((int)(end - start).TotalMinutes);
            session.Cost.Should().BeGreaterThan(0);
        }
    }
}