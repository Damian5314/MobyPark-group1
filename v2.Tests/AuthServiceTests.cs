using FluentAssertions;
using v2.Models;
using v2.Services;
using v2.Security;
using Xunit;


namespace v2.Tests
{
    public class AuthServiceTests
    {
        // ───────────────────────────────────────────────
        // REGISTRATION TESTS
        // ───────────────────────────────────────────────

        [Fact]
        public async Task Register_Should_Create_User_And_Return_Token()
        {
            var service = new AuthService();

            var req = new RegisterRequest
            {
                Username = "john",
                Password = "pass123",
                Name = "John Doe",
                Email = "john@example.com"
            };

            var response = await service.RegisterAsync(req);

            response.Should().NotBeNull();
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Register_Should_Hash_Password()
        {
            var service = new AuthService();

            var req = new RegisterRequest
            {
                Username = "hashTest",
                Password = "mypassword",
                Name = "Hash User",
                Email = "hash@test.com"
            };

            await service.RegisterAsync(req);

            // we need to fetch internal private list via reflection
            var usersField = typeof(AuthService).GetField("_users",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var users = usersField!.GetValue(service) as List<UserProfile>;

            var user = users!.First();

            user.Password.Should().NotBe("mypassword");   // should NOT be plain text
            PasswordHelper.VerifyPassword("mypassword", user.Password).Should().BeTrue();
        }

        [Fact]
        public async Task Register_Should_Throw_When_Username_Exists()
        {
            var service = new AuthService();

            var req = new RegisterRequest
            {
                Username = "duplicate",
                Password = "123",
                Name = "Dup",
                Email = "dup@test.com"
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
            var service = new AuthService();

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "loginUser",
                Password = "mypassword",
                Name = "Login User",
                Email = "login@test.com"
            });

            var result = await service.LoginAsync(new LoginRequest
            {
                Username = "loginUser",
                Password = "mypassword"
            });

            result.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_Should_Fail_With_Wrong_Password()
        {
            var service = new AuthService();

            await service.RegisterAsync(new RegisterRequest
            {
                Username = "wrongPass",
                Password = "correct",
                Name = "Wrong Pass User",
                Email = "wpass@test.com"
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
            var service = new AuthService();

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
