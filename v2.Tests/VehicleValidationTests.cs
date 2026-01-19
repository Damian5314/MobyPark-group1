using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using v2.Data;
using v2.Models;
using v2.Security;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class VehicleValidationTests
    {
        private readonly AppDbContext _context;
        private readonly VehicleService _vehicleService;
        private readonly AuthService _authService;
        private readonly UserProfileService _userService;

        public VehicleValidationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatabase(_context);

            var vehicleLoggerMock = new Mock<ILogger<VehicleService>>();
            var authLoggerMock = new Mock<ILogger<AuthService>>();
            var userLoggerMock = new Mock<ILogger<UserProfileService>>();

            _vehicleService = new VehicleService(_context, vehicleLoggerMock.Object);
            _authService = new AuthService(_context, authLoggerMock.Object);
            _userService = new UserProfileService(_context, userLoggerMock.Object);
        }

        private void SeedDatabase(AppDbContext context)
        {
            context.Users.Add(new UserProfile
            {
                Id = 1,
                Username = "testuser",
                Password = PasswordHelper.HashPassword("password123"),
                Name = "Test User",
                Email = "test@example.com",
                Phone = "0612345678",
                BirthYear = 1990,
                Role = "USER",
                Active = true,
                CreatedAt = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        [Theory]
        [InlineData("AB-12-34")]
        [InlineData("12-34-AB")]
        [InlineData("12-AB-34")]
        [InlineData("AB-12-CD")]
        [InlineData("AB-CD-12")]
        [InlineData("12-AB-CD")]
        [InlineData("12-ABC-3")]
        [InlineData("1-ABC-12")]
        [InlineData("AB-123-C")]
        [InlineData("A-123-BC")]
        [InlineData("ABC-12-D")]
        [InlineData("1-AB-123")]
        [InlineData("123-AB-4")]
        public async Task CreateVehicle_Should_Accept_Valid_Dutch_License_Plates(string licensePlate)
        {
            var vehicle = new Vehicle
            {
                UserId = 1,
                LicensePlate = licensePlate,
                Make = "Tesla",
                Model = "Model 3",
                Color = "Red",
                Year = 2022
            };

            var created = await _vehicleService.CreateAsync(vehicle);

            created.Should().NotBeNull();
            created.LicensePlate.Should().Be(licensePlate);
            created.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Vehicles_For_User()
        {
            var vehicle1 = new Vehicle
            {
                UserId = 1,
                LicensePlate = "AA-11-BB",
                Make = "Tesla",
                Model = "Model 3",
                Color = "Red",
                Year = 2022
            };

            var vehicle2 = new Vehicle
            {
                UserId = 1,
                LicensePlate = "BB-22-CC",
                Make = "BMW",
                Model = "i3",
                Color = "Blue",
                Year = 2021
            };

            await _vehicleService.CreateAsync(vehicle1);
            await _vehicleService.CreateAsync(vehicle2);

            var vehicles = await _vehicleService.GetByUserIdAsync(1);

            vehicles.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_Should_Return_Vehicle_When_Exists()
        {
            var vehicle = new Vehicle
            {
                UserId = 1,
                LicensePlate = "CC-33-DD",
                Make = "Tesla",
                Model = "Model Y",
                Color = "White",
                Year = 2023
            };

            var created = await _vehicleService.CreateAsync(vehicle);
            var retrieved = await _vehicleService.GetByIdAsync(created.Id);

            retrieved.Should().NotBeNull();
            retrieved!.LicensePlate.Should().Be("CC-33-DD");
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_Not_Found()
        {
            var retrieved = await _vehicleService.GetByIdAsync(99999);

            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Vehicle()
        {
            var vehicle = new Vehicle
            {
                UserId = 1,
                LicensePlate = "DD-44-EE",
                Make = "Tesla",
                Model = "Model X",
                Color = "Black",
                Year = 2022
            };

            var created = await _vehicleService.CreateAsync(vehicle);
            var deleted = await _vehicleService.DeleteAsync(created.Id);

            deleted.Should().BeTrue();

            var retrieved = await _vehicleService.GetByIdAsync(created.Id);
            retrieved.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Not_Found()
        {
            var deleted = await _vehicleService.DeleteAsync(99999);

            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Vehicle()
        {
            var vehicle = new Vehicle
            {
                UserId = 1,
                LicensePlate = "EE-55-FF",
                Make = "Tesla",
                Model = "Model 3",
                Color = "Red",
                Year = 2022
            };

            var created = await _vehicleService.CreateAsync(vehicle);

            created.Color = "Blue";
            created.Year = 2023;

            var updated = await _vehicleService.UpdateAsync(created.Id, created);

            updated.Should().NotBeNull();
            updated!.Color.Should().Be("Blue");
            updated.Year.Should().Be(2023);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Not_Found()
        {
            var vehicle = new Vehicle
            {
                UserId = 1,
                LicensePlate = "FF-66-GG",
                Make = "Tesla",
                Model = "Model 3",
                Color = "Red",
                Year = 2022
            };

            Func<Task> act = async () => await _vehicleService.UpdateAsync(99999, vehicle);

            await act.Should().ThrowAsync<System.Collections.Generic.KeyNotFoundException>()
                .WithMessage("Vehicle not found.");
        }
    }
}
