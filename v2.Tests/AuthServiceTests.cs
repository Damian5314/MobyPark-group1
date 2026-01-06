using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Models;
using v2.Security;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class AuthServiceTests
    {
        private readonly AuthService _service;
        private readonly AppDbContext _context;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            SeedDatabase(_context);

            _service = new AuthService(_context);
        }

        private void SeedDatabase(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed existing user for login tests
            context.Users.Add(
                new UserProfile
                {
                    Id = 1,
                    Username = "existinguser",
                    Password = PasswordHelper.HashPassword("testpassword"),
                    Role = "USER",
                    Name = "Existing User",
                    Email = "existing@test.com",
                    Phone = "+31612345678",
                    BirthYear = 1990,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task RegisterAsync_Should_Create_New_User_And_Return_Token()
        {
            var request = new RegisterRequest
            {
                Username = "newuser",
                Password = "password123",
                Name = "New User",
                Email = "newuser@test.com",
                Phone = "+31612345679",
                BirthYear = 1995
            };

            var response = await _service.RegisterAsync(request);

            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            user.Should().NotBeNull();
            user!.Name.Should().Be("New User");
            user.Email.Should().Be("newuser@test.com");
            user.Role.Should().Be("USER");
            user.Active.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_Should_Hash_Password()
        {
            var request = new RegisterRequest
            {
                Username = "userwithhash",
                Password = "mypassword",
                Name = "Hash User",
                Email = "hash@test.com",
                Phone = "+31612345680",
                BirthYear = 1992
            };

            await _service.RegisterAsync(request);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "userwithhash");
            user.Should().NotBeNull();
            user!.Password.Should().NotBe("mypassword"); // Password should be hashed
            PasswordHelper.VerifyPassword("mypassword", user.Password).Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_When_Username_Already_Exists()
        {
            var request = new RegisterRequest
            {
                Username = "existinguser",
                Password = "password123",
                Name = "Duplicate User",
                Email = "duplicate@test.com",
                Phone = "+31612345681",
                BirthYear = 1993
            };

            Func<Task> act = async () => await _service.RegisterAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username already exists.");
        }

        [Fact]
        public async Task RegisterAsync_Should_Throw_When_Email_Already_Exists()
        {
            var request = new RegisterRequest
            {
                Username = "uniqueuser",
                Password = "password123",
                Name = "Unique User",
                Email = "existing@test.com", // Email already exists
                Phone = "+31612345682",
                BirthYear = 1994
            };

            Func<Task> act = async () => await _service.RegisterAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already exists.");
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Token_For_Valid_Credentials()
        {
            var request = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var response = await _service.LoginAsync(request);

            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_For_Invalid_Username()
        {
            var request = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "testpassword"
            };

            Func<Task> act = async () => await _service.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task LoginAsync_Should_Throw_For_Invalid_Password()
        {
            var request = new LoginRequest
            {
                Username = "existinguser",
                Password = "wrongpassword"
            };

            Func<Task> act = async () => await _service.LoginAsync(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task LogoutAsync_Should_Invalidate_Token()
        {
            var loginRequest = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var loginResponse = await _service.LoginAsync(loginRequest);
            var token = loginResponse.Token;

            _service.IsTokenValid(token).Should().BeTrue();

            var result = await _service.LogoutAsync(token);

            result.Should().BeTrue();
            _service.IsTokenValid(token).Should().BeFalse();
        }

        [Fact]
        public async Task LogoutAsync_Should_Return_False_For_Invalid_Token()
        {
            var result = await _service.LogoutAsync("invalidtoken");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTokenValid_Should_Return_True_For_Valid_Token()
        {
            var loginRequest = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var loginResponse = await _service.LoginAsync(loginRequest);

            _service.IsTokenValid(loginResponse.Token).Should().BeTrue();
        }

        [Fact]
        public async Task IsTokenValid_Should_Return_False_For_Invalid_Token()
        {
            _service.IsTokenValid("invalidtoken").Should().BeFalse();
        }

        [Fact]
        public async Task GetUsernameFromToken_Should_Return_Username()
        {
            var loginRequest = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var loginResponse = await _service.LoginAsync(loginRequest);

            var username = _service.GetUsernameFromToken(loginResponse.Token);

            username.Should().Be("existinguser");
        }

        [Fact]
        public async Task GetUsernameFromToken_Should_Return_Null_For_Invalid_Token()
        {
            var username = _service.GetUsernameFromToken("invalidtoken");

            username.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveTokenForUser_Should_Return_Token()
        {
            var loginRequest = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var loginResponse = await _service.LoginAsync(loginRequest);

            var token = _service.GetActiveTokenForUser("existinguser");

            token.Should().Be(loginResponse.Token);
        }

        [Fact]
        public async Task GetActiveTokenForUser_Should_Return_Null_For_User_Without_Session()
        {
            var token = _service.GetActiveTokenForUser("nonexistentuser");

            token.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_Should_Replace_Old_Session()
        {
            var loginRequest = new LoginRequest
            {
                Username = "existinguser",
                Password = "testpassword"
            };

            var firstResponse = await _service.LoginAsync(loginRequest);
            var firstToken = firstResponse.Token;

            var secondResponse = await _service.LoginAsync(loginRequest);
            var secondToken = secondResponse.Token;

            _service.IsTokenValid(firstToken).Should().BeFalse(); // Old token invalidated
            _service.IsTokenValid(secondToken).Should().BeTrue(); // New token valid
        }

        [Fact]
        public async Task RegisterAsync_Should_Set_CreatedAt_Timestamp()
        {
            var request = new RegisterRequest
            {
                Username = "timestampuser",
                Password = "password123",
                Name = "Timestamp User",
                Email = "timestamp@test.com",
                Phone = "+31612345683",
                BirthYear = 1996
            };

            var beforeRegistration = DateTime.UtcNow;
            await _service.RegisterAsync(request);
            var afterRegistration = DateTime.UtcNow;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "timestampuser");
            user.Should().NotBeNull();
            user!.CreatedAt.Should().BeOnOrAfter(beforeRegistration.AddSeconds(-1));
            user.CreatedAt.Should().BeOnOrBefore(afterRegistration.AddSeconds(1));
        }
    }
}
