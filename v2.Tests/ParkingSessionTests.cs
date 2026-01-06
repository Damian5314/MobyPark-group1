using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class ParkingSessionServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ParkingSessionService _service;

        public ParkingSessionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ParkingSessionDb_{Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(options);

            SeedDatabase(_context);

            // PaymentService not needed for these tests
            var fakePaymentService = new FakePaymentService();

            _service = new ParkingSessionService(_context, fakePaymentService);
        }

        private static void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureCreated();

            context.ParkingLots.Add(new ParkingLot
            {
                Id = 1,
                Name = "Test Lot",
                Capacity = 10,
                Reserved = 0,
                Tariff = 2.5m,
                DayTariff = 20m,
                CreatedAt = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task StartSession_Should_Create_New_Session()
        {
            var session = await _service.StartSessionAsync(
                parkingLotId: 1,
                licensePlate: "AA-11-BB",
                username: "testuser"
            );

            session.Should().NotBeNull();
            session.LicensePlate.Should().Be("AA-11-BB");
            session.Username.Should().Be("testuser");
            session.Stopped.Should().Be(default);
        }

        [Fact]
        public async Task StartSession_Should_Fail_When_Lot_Is_Full()
        {
            var lot = await _context.ParkingLots.FirstAsync();
            lot.Reserved = lot.Capacity;
            await _context.SaveChangesAsync();

            Func<Task> act = async () =>
                await _service.StartSessionAsync(1, "AA-11-BB", "user");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Parking lot is full");
        }

        [Fact]
        public async Task StopSession_Should_Set_Stopped_And_Cost()
        {
            var session = await _service.StartSessionAsync(1, "AA-11-BB", "user");

            var stopped = await _service.StopSessionAsync(session.Id);

            stopped.Stopped.Should().NotBe(default);
            stopped.DurationMinutes.Should().BeGreaterThanOrEqualTo(0);
            stopped.Cost.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateFromReservation_Should_Create_Completed_Session()
        {
            var start = DateTime.UtcNow.AddHours(-2);
            var end = DateTime.UtcNow;

            var session = await _service.CreateFromReservationAsync(
                1,
                "AA-11-BB",
                "user",
                start,
                end
            );

            session.Started.Should().Be(start);
            session.Stopped.Should().Be(end);
            session.DurationMinutes.Should().Be(120);
            session.Cost.Should().BeGreaterThan(0);
        }
    }

    // Minimal fake to satisfy constructor
    internal class FakePaymentService : IPaymentService
    {
        public Task<Payment> CreateAsync(PaymentCreateDto dto) =>
            throw new NotImplementedException();

        public Task<Payment> PaySingleSessionAsync(PaySingleSessionDto dto) =>
            throw new NotImplementedException();

        public Task<IEnumerable<Payment>> GetAllAsync() =>
            throw new NotImplementedException();

        public Task<Payment?> GetByIdAsync(int id) =>
            throw new NotImplementedException();

        public Task<IEnumerable<Payment>> GetByInitiatorAsync(string initiator) =>
            throw new NotImplementedException();

        public Task<IEnumerable<ParkingSession>> GetUnpaidSessionsAsync(string licensePlate) =>
            throw new NotImplementedException();

        public Task<bool> DeleteAsync(int id) =>
            throw new NotImplementedException();
    }
}