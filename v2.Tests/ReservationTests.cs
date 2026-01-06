using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class ReservationServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ReservationService _service;

        public ReservationServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ReservationDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);

            SeedDatabase(_context);

            var parkingSessionService = new FakeParkingSessionService();

            _service = new ReservationService(_context, parkingSessionService);
        }

        private static void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureCreated();

            context.ParkingLots.Add(new ParkingLot
            {
                Id = 1,
                Name = "Lot 1",
                Capacity = 5,
                Reserved = 0,
                Tariff = 2,
                DayTariff = 20,
                CreatedAt = DateTime.UtcNow
            });

            context.Vehicles.Add(new Vehicle
            {
                Id = 1,
                LicensePlate = "AA-11-BB"
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task Create_Should_Create_Reservation()
        {
            var dto = new ReservationCreateDto
            {
                UserId = 1,
                ParkingLotId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var reservation = await _service.CreateAsync(dto);

            reservation.Should().NotBeNull();
            reservation.Status.Should().Be("Active");
        }

        [Fact]
        public async Task Update_To_Confirmed_Should_Create_ParkingSession()
        {
            var reservation = await _service.CreateAsync(new ReservationCreateDto
            {
                UserId = 1,
                ParkingLotId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            });

            var updateDto = new ReservationCreateDto
            {
                VehicleId = 1,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = "Confirmed"
            };

            var updated = await _service.UpdateAsync(reservation.Id, updateDto);

            updated.Status.Should().Be("Confirmed");
        }

        [Fact]
        public async Task Delete_Should_Remove_Reservation()
        {
            var reservation = await _service.CreateAsync(new ReservationCreateDto
            {
                UserId = 1,
                ParkingLotId = 1,
                VehicleId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            });

            var result = await _service.DeleteAsync(reservation.Id);

            result.Should().BeTrue();
            (await _context.Reservations.FindAsync(reservation.Id))
                .Should().BeNull();
        }
    }

    internal class FakeParkingSessionService : IParkingSessionService
    {
        public Task<ParkingSession> CreateFromReservationAsync(
            int parkingLotId,
            string licensePlate,
            string username,
            DateTime startTime,
            DateTime endTime)
        {
            return Task.FromResult(new ParkingSession
            {
                ParkingLotId = parkingLotId,
                LicensePlate = licensePlate,
                Username = username,
                Started = startTime,
                Stopped = endTime
            });
        }

        public Task<ParkingSession> StartSessionAsync(int parkingLotId, string licensePlate, string username) =>
            throw new NotImplementedException();

        public Task<ParkingSession> StopSessionAsync(int sessionId) =>
            throw new NotImplementedException();

        public Task<ParkingSession?> GetByIdAsync(int sessionId) =>
            throw new NotImplementedException();

        public Task<IEnumerable<ParkingSession>> GetActiveSessionsAsync() =>
            throw new NotImplementedException();
    }
}