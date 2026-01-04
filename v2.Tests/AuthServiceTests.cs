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
        private AuthService CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var db = new AppDbContext(options);
            return new AuthService(db);
        }

        // REGISTER TESTS
        [Fact]
        public async Task Register_Should_Create_User_And_Return_Token()
        {
            // Arrange
            var testDbName = nameof(Register_Should_Create_User_And_Return_Token) + "_" + Guid.NewGuid().ToString("N");

            // IMPORTANT: use the SAME DbContextOptions for both service + verification db
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(testDbName)
                .Options;

            await using var db = new AppDbContext(options);

            // If AuthService needs other deps in your project, add them here
            // (This version only depends on AppDbContext like your other services do.)
            var service = new AuthService(db);

            var req = new RegisterRequest
            {
                Username = "usertest",
                Password = "usertest",
                Name = "User One",
                Email = "user@test.com",
                Phone = "+310000001",
                BirthYear = 1995
            };

            // Act
            var response = await service.RegisterAsync(req);

            // Assert: token returned
            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

            // Assert: user inserted in DB
            // (use AsNoTracking to avoid any EF tracking weirdness)
            var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Username == req.Username);

            user.Should().NotBeNull("RegisterAsync should insert the new user into the database");

            // Assert: stored fields match request
            user!.Username.Should().Be(req.Username);
            user.Name.Should().Be(req.Name);
            user.Email.Should().Be(req.Email);
            user.Phone.Should().Be(req.Phone);
            user.BirthYear.Should().Be(req.BirthYear);

            // Assert: password is hashed and verifies
            PasswordHelper.VerifyPassword(req.Password, user.Password).Should().BeTrue();

            // Optional defaults (adjust/remove if your AuthService sets different defaults)
            user.Role.Should().NotBeNullOrWhiteSpace();
            user.Active.Should().BeTrue();
        }

        [Fact]
        public async Task Register_Should_Throw_When_Username_Exists()
        {
            var service = CreateService(nameof(Register_Should_Throw_When_Username_Exists));

            var req = new RegisterRequest
            {
                Username = "duplicate",
                Password = "pass123",
                Name = "Dup User",
                Email = "dup@test.com",
                Phone = "+310000002",
                BirthYear = 1990
            };

            await service.RegisterAsync(req);

            await service.Invoking(s => s.RegisterAsync(req))
                         .Should()
                         .ThrowAsync<InvalidOperationException>()
                         .WithMessage("Username already exists.");
        }

        // LOGIN TESTS
        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Correct()
        {
            var service = CreateService(nameof(Login_Should_Return_Token_When_Credentials_Correct));

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "loginUser",
                Password = "password123",
                Name = "Login User",
                Email = "login@test.com",
                Phone = "+310000003",
                BirthYear = 1992
            });

            var result = await service.LoginAsync(new LoginRequest
            {
                Username = "loginUser",
                Password = "password123"
            });

            result.Token.Should().NotBeNullOrWhiteSpace();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

            //session info
            service.GetUsernameFromToken(result.Token).Should().Be("loginUser");
            service.IsTokenValid(result.Token).Should().BeTrue();
            service.GetActiveTokenForUser("loginUser").Should().Be(result.Token);
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            var service = CreateService(nameof(Login_Should_Fail_With_Wrong_Password));

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "wrongPass",
                Password = "correct123",
                Name = "Wrong Pass User",
                Email = "wpass@test.com",
                Phone = "+310000004",
                BirthYear = 1988
            });

            await service.Invoking(s => s.LoginAsync(new LoginRequest
            {
                Username = "wrongPass",
                Password = "incorrect"
            })).Should()
              .ThrowAsync<UnauthorizedAccessException>()
              .WithMessage("Invalid username or password.");
        }

        // LOGOUT & TOKEN TESTS
        [Fact]
        public async Task Logout_Should_Invalidate_Token()
        {
            var service = CreateService(nameof(Logout_Should_Invalidate_Token));

            var response = await service.RegisterAsync(new RegisterRequest
            {
                Username = "logoutUser",
                Password = "pass123",
                Name = "Logout User",
                Email = "logout@test.com",
                Phone = "+310000005",
                BirthYear = 1990
            });

            var token = response.Token;

            service.IsTokenValid(token).Should().BeTrue();

            var logoutResult = await service.LogoutAsync(token);
            logoutResult.Should().BeTrue();

            service.IsTokenValid(token).Should().BeFalse();
            service.GetUsernameFromToken(token).Should().BeNull();
            service.GetActiveTokenForUser("logoutUser").Should().BeNull();
        }

        [Fact]
        public async Task LogoutCurrentUserAsync_Should_Clear_CurrentUserToken()
        {
            var service = CreateService(nameof(LogoutCurrentUserAsync_Should_Clear_CurrentUserToken));

            var response = await service.RegisterAsync(new RegisterRequest
            {
                Username = "currentUser",
                Password = "pass123",
                Name = "Current User",
                Email = "current@test.com",
                Phone = "+310000006",
                BirthYear = 1993
            });

            var result = await service.LogoutCurrentUserAsync();
            result.Should().BeTrue();

            //token not valid anymore
            service.IsTokenValid(response.Token).Should().BeFalse();
            service.GetCurrentUsername().Should().BeNull();
        }
    }
}
