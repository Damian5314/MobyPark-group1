using System;
using System.ComponentModel.DataAnnotations;
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
    public class RegistrationValidationTests
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public RegistrationValidationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var loggerMock = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_context, loggerMock.Object);
        }

        [Theory]
        [InlineData("0612345678")]
        [InlineData("0698765432")]
        [InlineData("0600000000")]
        [InlineData("0699999999")]
        [InlineData("0611111111")]
        [InlineData("0622222222")]
        [InlineData("0633333333")]
        [InlineData("0644444444")]
        [InlineData("0655555555")]
        public async Task RegisterAsync_Should_Accept_Valid_Dutch_Phone_Numbers(string phoneNumber)
        {
            var request = new RegisterRequest
            {
                Username = $"user_{phoneNumber}",
                Password = "password123",
                Name = "Test User",
                Email = $"{phoneNumber}@test.com",
                Phone = phoneNumber,
                BirthYear = 1990
            };

            var response = await _authService.RegisterAsync(request);

            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phoneNumber);
            user.Should().NotBeNull();
            user!.Phone.Should().Be(phoneNumber);
        }

        [Theory]
        [InlineData("1234567890")]      // Doesn't start with 06
        [InlineData("0123456789")]      // Doesn't start with 06
        [InlineData("06123456")]        // Too short
        [InlineData("061234567890")]    // Too long
        [InlineData("06-12345678")]     // Contains dash
        [InlineData("06 12345678")]     // Contains space
        [InlineData("+31612345678")]    // Contains + and country code
        [InlineData("06abcd1234")]      // Contains letters
        public void RegisterRequest_Should_Reject_Invalid_Phone_Numbers(string phoneNumber)
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Password = "password123",
                Name = "Test User",
                Email = "test@test.com",
                Phone = phoneNumber,
                BirthYear = 1990
            };

            var context = new ValidationContext(request);
            var results = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            isValid.Should().BeFalse();
            results.Should().HaveCountGreaterThan(0);
            results.Should().Contain(r => r.ErrorMessage != null && r.ErrorMessage.Contains("06"));
        }

        [Fact]
        public async Task RegisterAsync_Should_Create_User_With_Active_Status()
        {
            var request = new RegisterRequest
            {
                Username = "activeuser",
                Password = "password123",
                Name = "Active User",
                Email = "active@test.com",
                Phone = "0677777777",
                BirthYear = 1995
            };

            await _authService.RegisterAsync(request);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "activeuser");
            user.Should().NotBeNull();
            user!.Active.Should().BeTrue();
            user.Role.Should().Be("USER");
        }

        [Fact]
        public async Task RegisterAsync_Should_Set_Default_Role_To_USER()
        {
            var request = new RegisterRequest
            {
                Username = "roleuser",
                Password = "password123",
                Name = "Role User",
                Email = "role@test.com",
                Phone = "0688888888",
                BirthYear = 1992
            };

            await _authService.RegisterAsync(request);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "roleuser");
            user.Should().NotBeNull();
            user!.Role.Should().Be("USER");
        }

        [Fact]
        public void RegisterRequest_Should_Require_All_Fields()
        {
            var request = new RegisterRequest
            {
                Username = "",
                Password = "",
                Name = "",
                Email = "",
                Phone = "",
                BirthYear = 0
            };

            var context = new ValidationContext(request);
            var results = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            isValid.Should().BeFalse();
            results.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void RegisterRequest_Should_Validate_Email_Format()
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Password = "password123",
                Name = "Test User",
                Email = "invalid-email",
                Phone = "0612345678",
                BirthYear = 1990
            };

            var context = new ValidationContext(request);
            var results = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterRequest_Should_Require_Minimum_Password_Length()
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Password = "123",
                Name = "Test User",
                Email = "test@test.com",
                Phone = "0612345678",
                BirthYear = 1990
            };

            var context = new ValidationContext(request);
            var results = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Theory]
        [InlineData(1899)]
        [InlineData(2026)]
        [InlineData(2100)]
        public void RegisterRequest_Should_Validate_Birth_Year_Range(int birthYear)
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Password = "password123",
                Name = "Test User",
                Email = "test@test.com",
                Phone = "0612345678",
                BirthYear = birthYear
            };

            var context = new ValidationContext(request);
            var results = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("BirthYear"));
        }
    }
}
