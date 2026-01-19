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
    public class BillingTests
    {
        private readonly BillingService _service;
        private readonly AppDbContext _context;

        public BillingTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            SeedDatabase(_context);

            var loggerMock = new Mock<ILogger<BillingService>>();
            _service = new BillingService(_context, loggerMock.Object);
        }

        private void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed Users
            context.Users.AddRange(
                new UserProfile
                {
                    Id = 1,
                    Username = "user1",
                    Password = "password123",
                    Role = "User",
                    Name = "Test User 1",
                    Email = "user1@test.com",
                    Phone = "0612345678",
                    BirthYear = 1990,
                    Active = true
                },
                new UserProfile
                {
                    Id = 2,
                    Username = "user2",
                    Password = "password123",
                    Role = "User",
                    Name = "Test User 2",
                    Email = "user2@test.com",
                    Phone = "0612345679",
                    BirthYear = 1985,
                    Active = true
                },
                new UserProfile
                {
                    Id = 99999,
                    Username = "user_nonexistent",
                    Password = "password123",
                    Role = "User",
                    Name = "Nonexistent User",
                    Email = "nonexistent@test.com",
                    Phone = "0600000000",
                    BirthYear = 1980,
                    Active = true
                }
            );

            // Seed Payments
            context.Payments.AddRange(
                new Payment
                {
                    Id = 1,
                    Amount = 10.50m,
                    Transaction = "trans1",
                    Hash = "hash1",
                    Initiator = "user1",
                    CreatedAt = DateTime.UtcNow,
                    Completed = DateTime.UtcNow,
                    SessionId = "1",
                    TData = new TData
                    {
                        Amount = 10.50m,
                        Date = DateTime.UtcNow,
                        Method = "iDEAL",
                        Issuer = "user1",
                        Bank = "ING"
                    }
                },
                new Payment
                {
                    Id = 2,
                    Amount = 20.00m,
                    Transaction = "trans2",
                    Hash = "hash2",
                    Initiator = "user2",
                    CreatedAt = DateTime.UtcNow,
                    Completed = DateTime.UtcNow,
                    SessionId = "2",
                    TData = new TData
                    {
                        Amount = 20.00m,
                        Date = DateTime.UtcNow,
                        Method = "Credit Card",
                        Issuer = "user2",
                        Bank = "Rabobank"
                    }
                },
                new Payment
                {
                    Id = 3,
                    Amount = 15.75m,
                    Transaction = "trans3",
                    Hash = "hash3",
                    Initiator = "user1",
                    CreatedAt = DateTime.UtcNow,
                    Completed = DateTime.UtcNow,
                    SessionId = "3",
                    TData = new TData
                    {
                        Amount = 15.75m,
                        Date = DateTime.UtcNow,
                        Method = "iDEAL",
                        Issuer = "user1",
                        Bank = "ABN AMRO"
                    }
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Billings_Grouped_By_Initiator()
        {
            var billings = await _service.GetAllAsync();

            billings.Should().NotBeNull();
            billings.Should().HaveCount(2); // user1 and user2

            var user1Billing = billings.FirstOrDefault(b => b.User == "user1");
            user1Billing.Should().NotBeNull();
            user1Billing!.Payments.Should().HaveCount(2); // 2 payments for user1
            user1Billing.TotalAmount.Should().Be(26.25m); // 10.50 + 15.75

            var user2Billing = billings.FirstOrDefault(b => b.User == "user2");
            user2Billing.Should().NotBeNull();
            user2Billing!.Payments.Should().HaveCount(1); // 1 payment for user2
            user2Billing.TotalAmount.Should().Be(20.00m);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Billing_For_Existing_User()
        {
            var billing = await _service.GetByUserIdAsync(1);

            billing.Should().NotBeNull();
            billing!.User.Should().Be("user1");
            billing.Payments.Should().HaveCount(2);
            billing.TotalAmount.Should().Be(26.25m);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Null_For_Nonexistent_User()
        {
            var billing = await _service.GetByUserIdAsync(12345);

            billing.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Null_For_User_Without_Payments()
        {
            var billing = await _service.GetByUserIdAsync(99999);

            billing.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Correct_Payment_Details()
        {
            var billing = await _service.GetByUserIdAsync(2);

            billing.Should().NotBeNull();
            billing!.User.Should().Be("user2");
            billing.Payments.Should().HaveCount(1);

            var payment = billing.Payments.First();
            payment.Amount.Should().Be(20.00m);
            payment.Initiator.Should().Be("user2");
        }
    }
}
