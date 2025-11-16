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

        // ───────────────────────────────────────────────
        // REGISTRATION TESTS
        // ───────────────────────────────────────────────

        [Fact]
        public async Task Register_Should_Create_User_And_Return_Token()
        {
            var service = CreateService(nameof(Register_Should_Create_User_And_Return_Token));

            var req = new RegisterRequest
            {
                Username = "johntest",
                Password = "johntest",
                Name = "johntest",
                Email = "johntest@example.com",
                Phone = "+310685543241",
                BirthYear = 2005
            };

            var response = await service.RegisterAsync(req);

            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().NotBeNullOrWhiteSpace(); // string now
        }

        [Fact]
        public async Task Register_Should_Hash_Password()
        {
            var service = CreateService(nameof(Register_Should_Hash_Password));

            var req = new RegisterRequest
            {
                Username = "hashTest",
                Password = "mypassword",
                Name = "Hash User",
                Email = "hash@test.com",
                Phone = "+310000000",
                BirthYear = 2000
            };

            await service.RegisterAsync(req);

            // Get the underlying DbContext to check stored user
            var dbField = typeof(AuthService).GetField("_db",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var db = (AppDbContext)dbField!.GetValue(service)!;

            var user = db.Users.First(u => u.Username == "hashTest");

            user.Password.Should().NotBe("mypassword"); // not plain text
            PasswordHelper.VerifyPassword("mypassword", user.Password).Should().BeTrue();
        }

        [Fact]
        public async Task Register_Should_Throw_When_Username_Exists()
        {
            var service = CreateService(nameof(Register_Should_Throw_When_Username_Exists));

            var req = new RegisterRequest
            {
                Username = "duplicate",
                Password = "123456",
                Name = "Dup User",
                Email = "dup@test.com",
                Phone = "+310000001",
                BirthYear = 1999
            };

            await service.RegisterAsync(req);

            var act = async () => await service.RegisterAsync(req);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username already exists.");
        }

        // ───────────────────────────────────────────────
        // LOGIN TESTS
        // ───────────────────────────────────────────────

        [Fact]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
        {
            var service = CreateService(nameof(Login_Should_Return_Token_When_Credentials_Are_Correct));

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "loginUser",
                Password = "mypassword",
                Name = "Login User",
                Email = "login@test.com",
                Phone = "+310000002",
                BirthYear = 1995
            });

            var result = await service.LoginAsync(new LoginRequest
            {
                Username = "loginUser",
                Password = "mypassword"
            });

            result.Token.Should().NotBeNullOrWhiteSpace();
            result.ExpiresAt.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            var service = CreateService(nameof(Login_Should_Fail_With_Wrong_Password));

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "wrongPass",
                Password = "correct",
                Name = "Wrong Pass User",
                Email = "wpass@test.com",
                Phone = "+310000003",
                BirthYear = 1990
            });

            var act = async () => await service.LoginAsync(new LoginRequest
            {
                Username = "wrongPass",
                Password = "incorrect"
            });

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Invalid username or password.");
        }

        [Fact]
        public async Task Login_Should_Fail_When_User_Does_Not_Exist()
        {
            var service = CreateService(nameof(Login_Should_Fail_When_User_Does_Not_Exist));

            var act = async () => await service.LoginAsync(new LoginRequest
            {
                Username = "unknownUser",
                Password = "anything"
            });

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Invalid username or password.");
        }
    }
}
