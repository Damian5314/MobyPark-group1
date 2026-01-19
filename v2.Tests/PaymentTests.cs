using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using v2.Data;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class PaymentTests
    {
        private readonly PaymentService _service;
        private readonly AppDbContext _context;

        public PaymentTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            SeedDatabase(_context);

            var loggerMock = new Mock<ILogger<PaymentService>>();
            _service = new PaymentService(_context, loggerMock.Object);
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
                    Capacity = 10,
                    Reserved = 0,
                    Address = "123 Main St",
                    Location = "City Center",
                    Tariff = 5,
                    DayTariff = 20
                }
            );

            // Seed Parking Sessions
            context.ParkingSessions.AddRange(
                new ParkingSession
                {
                    Id = 1,
                    ParkingLotId = 1,
                    LicensePlate = "XX-YY-99",
                    Username = "testuser",
                    Started = DateTime.UtcNow.AddHours(-2),
                    Stopped = DateTime.UtcNow.AddHours(-1),
                    DurationMinutes = 60,
                    Cost = 5.00m,
                    PaymentStatus = "Pending"
                },
                new ParkingSession
                {
                    Id = 2,
                    ParkingLotId = 1,
                    LicensePlate = "AB-CD-12",
                    Username = "user1",
                    Started = DateTime.UtcNow.AddHours(-3),
                    Stopped = DateTime.UtcNow.AddHours(-2),
                    DurationMinutes = 60,
                    Cost = 5.00m,
                    PaymentStatus = "Pending"
                }
            );

            context.SaveChanges();

            // Seed Payments (after SaveChanges to avoid ID conflicts)
            context.Payments.AddRange(
                new Payment
                {
                    Amount = 10.50m,
                    Transaction = "trans1",
                    Hash = "hash1",
                    Initiator = "testuser",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Completed = DateTime.UtcNow.AddDays(-1),
                    SessionId = "100",
                    TData = new TData
                    {
                        Amount = 10.50m,
                        Date = DateTime.UtcNow.AddDays(-1),
                        Method = "iDEAL",
                        Issuer = "testuser",
                        Bank = "ING"
                    }
                },
                new Payment
                {
                    Amount = 20.00m,
                    Transaction = "trans2",
                    Hash = "hash2",
                    Initiator = "user1",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Completed = DateTime.UtcNow.AddDays(-2),
                    SessionId = "200",
                    TData = new TData
                    {
                        Amount = 20.00m,
                        Date = DateTime.UtcNow.AddDays(-2),
                        Method = "Credit Card",
                        Issuer = "user1",
                        Bank = "Rabobank"
                    }
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAll_Should_Return_List_Of_Payments()
        {
            var payments = await _service.GetAllAsync();

            payments.Should().NotBeNull();
            payments.Should().HaveCount(2);
            payments.Should().BeInDescendingOrder(p => p.CreatedAt);
        }

        [Fact]
        public async Task GetById_Should_Return_Payment_When_Exists()
        {
            var allPayments = await _service.GetAllAsync();
            var firstPayment = allPayments.First();

            var payment = await _service.GetByIdAsync(firstPayment.Id);

            payment.Should().NotBeNull();
            payment!.Id.Should().Be(firstPayment.Id);
            payment.Initiator.Should().Be("testuser");
            payment.Amount.Should().Be(10.50m);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_For_Nonexistent_Payment()
        {
            var payment = await _service.GetByIdAsync(99999);

            payment.Should().BeNull();
        }

        [Fact]
        public async Task GetByInitiator_Should_Return_Payments_For_User()
        {
            var payments = await _service.GetByInitiatorAsync("testuser");

            payments.Should().NotBeNull();
            payments.Should().HaveCount(1);
            payments.First().Initiator.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByInitiator_Should_Return_Empty_For_Unknown_User()
        {
            var payments = await _service.GetByInitiatorAsync("unknownuser");

            payments.Should().NotBeNull();
            payments.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnpaidSessions_Should_Return_List_Of_Sessions()
        {
            var sessions = await _service.GetUnpaidSessionsAsync("XX-YY-99");

            sessions.Should().NotBeNull();
            sessions.Should().HaveCount(1);
            sessions.First().LicensePlate.Should().Be("XX-YY-99");
            sessions.First().PaymentStatus.Should().Be("Pending");
        }

        [Fact]
        public async Task GetUnpaidSessions_Should_Return_Empty_For_Unknown_LicensePlate()
        {
            var sessions = await _service.GetUnpaidSessionsAsync("ZZ-ZZ-00");

            sessions.Should().NotBeNull();
            sessions.Should().BeEmpty();
        }

        [Fact]
        public async Task Delete_Should_Remove_Payment_When_Exists()
        {
            var allPayments = await _service.GetAllAsync();
            var paymentToDelete = allPayments.First();

            var result = await _service.DeleteAsync(paymentToDelete.Id);

            result.Should().BeTrue();

            var payment = await _context.Payments.FindAsync(paymentToDelete.Id);
            payment.Should().BeNull();
        }

        [Fact]
        public async Task Delete_Should_Return_False_For_Nonexistent_Payment()
        {
            var result = await _service.DeleteAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PaySingleSession_Should_Create_Payment_And_Update_Session()
        {
            var dto = new PaySingleSessionDto
            {
                SessionId = 1,
                LicensePlate = "XX-YY-99",
                Initiator = "testuser",
                Method = "iDEAL",
                Bank = "ING"
            };

            var payment = await _service.PaySingleSessionAsync(dto);

            payment.Should().NotBeNull();
            payment.Amount.Should().Be(5.00m);
            payment.Initiator.Should().Be("testuser");
            payment.SessionId.Should().Be("1");

            var session = await _context.ParkingSessions.FindAsync(1);
            session.Should().NotBeNull();
            session!.PaymentStatus.Should().Be("Completed");
            session.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task PaySingleSession_Should_Throw_For_Nonexistent_Session()
        {
            var dto = new PaySingleSessionDto
            {
                SessionId = 99999,
                LicensePlate = "XX-YY-99",
                Initiator = "testuser",
                Method = "iDEAL",
                Bank = "ING"
            };

            Func<Task> act = async () => await _service.PaySingleSessionAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No unpaid session found for this license plate and session ID");
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Payment_For_All_Unpaid_Sessions()
        {
            var dto = new PaymentCreateDto
            {
                LicensePlate = "XX-YY-99",
                Initiator = "testuser",
                Method = "iDEAL",
                Bank = "ING"
            };

            var payment = await _service.CreateAsync(dto);

            payment.Should().NotBeNull();
            payment.Amount.Should().Be(5.00m); // Total of all unpaid sessions for XX-YY-99
            payment.Initiator.Should().Be("testuser");

            var session = await _context.ParkingSessions.FindAsync(1);
            session.Should().NotBeNull();
            session!.PaymentStatus.Should().Be("Paid");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_No_Unpaid_Sessions_Found()
        {
            var dto = new PaymentCreateDto
            {
                LicensePlate = "ZZ-ZZ-00",
                Initiator = "testuser",
                Method = "iDEAL",
                Bank = "ING"
            };

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No unpaid parking sessions found for license plate ZZ-ZZ-00");
        }
    }
}
