using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using v2.Models;
using Xunit;

namespace v2.Tests
{
    public class UserProfileTests : TestBase
    {
        private const string RegisterUrl = "/api/Auth/register"; // <-- change if your route differs

        // Generate unique username for each test run
        private static string UniqueUsername(string prefix = "user") => $"{prefix}_{Guid.NewGuid():N}";

        // Matches the register JSON you provided
        private class RegisterRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
            public int BirthYear { get; set; }
        }

        // Common register response shape in many projects
        // If your API returns different JSON, adjust this DTO accordingly.
        private class AuthResponse
        {
            public string Token { get; set; } = "";
        }

        private class UpdateMyProfileRequest
        {
            public string Username { get; set; } = "";
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string? Phone { get; set; }
            public int? BirthYear { get; set; }
        }

        private class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = "";
            public string NewPassword { get; set; } = "";
        }

        private class MessageResponse
        {
            public string Message { get; set; } = "";
        }

        private class UpdateProfileResponse
        {
            public string Message { get; set; } = "";
            public UserProfile Profile { get; set; } = default!;
        }

        private async Task<string> RegisterAndGetTokenAsync(string username, string password = "test123")
        {
            var req = new RegisterRequest
            {
                Username = username,
                Password = password,
                Name = "Test User",
                Email = $"{username}@example.com",
                Phone = "123456",
                BirthYear = 1990
            };

            var res = await Client.PostAsJsonAsync(RegisterUrl, req);

            // If tests run in parallel and username already exists, you may get Conflict.
            // Better: ensure unique usernames per test, or reset DB between tests.
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
            auth.Should().NotBeNull();
            auth!.Token.Should().NotBeNullOrWhiteSpace();

            return auth.Token;
        }

        private void UseToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // GET /api/UserProfile/me
        [Fact]
        public async Task User_Can_Get_Own_Profile()
        {
            var username = UniqueUsername("testuser");
            var token = await RegisterAndGetTokenAsync(username);
            UseToken(token);

            var res = await Client.GetAsync("/api/UserProfile/me");
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await res.Content.ReadFromJsonAsync<UserProfile>();
            user.Should().NotBeNull();
            user!.Username.Should().Be(username);
        }

        // PUT /api/UserProfile/me
        [Fact]
        public async Task User_Can_Update_Own_Profile()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser2"));
            UseToken(token);

            var update = new UpdateMyProfileRequest
            {
                Username = "testuser2",
                Name = "Updated User",
                Email = "updated@example.com",
                Phone = "999999",
                BirthYear = 1995
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/me", update);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await res.Content.ReadFromJsonAsync<UpdateProfileResponse>();
            body.Should().NotBeNull();
            body!.Message.Should().Be("Profile updated successfully.");

            body.Profile.Should().NotBeNull();
            body.Profile.Username.Should().Be("testuser2");
            body.Profile.Name.Should().Be("Updated User");
            body.Profile.Email.Should().Be("updated@example.com");
            body.Profile.Phone.Should().Be("999999");
            body.Profile.BirthYear.Should().Be(1995);
        }

        // PUT /api/UserProfile/me/password
        [Fact]
        public async Task User_Can_Change_Password()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser3"), password: "test123");
            UseToken(token);

            var req = new ChangePasswordRequest
            {
                CurrentPassword = "test123",
                NewPassword = "newpass"
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/me/password", req);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var payload = await res.Content.ReadFromJsonAsync<MessageResponse>();
            payload.Should().NotBeNull();
            payload!.Message.Should().Be("Password changed successfully.");
        }

        [Fact]
        public async Task Wrong_CurrentPassword_Should_Fail()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser4"), password: "test123");
            UseToken(token);

            var req = new ChangePasswordRequest
            {
                CurrentPassword = "WRONG",
                NewPassword = "newpass"
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/me/password", req);
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // DELETE /api/UserProfile/me
        [Fact]
        public async Task User_Can_Delete_Own_Profile()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser5"));
            UseToken(token);

            var res = await Client.DeleteAsync("/api/UserProfile/me");
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var text = await res.Content.ReadAsStringAsync();
            text.Should().Contain("Account deleted");
        }

        // ADMIN-only endpoints should be blocked for normal users
        [Fact]
        public async Task User_Cannot_Access_Admin_Endpoints()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser6"));
            UseToken(token);

            var getRes = await Client.GetAsync("/api/UserProfile/testuser6");
            getRes.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

            var putRes = await Client.PutAsJsonAsync("/api/UserProfile/testuser6", new
            {
                name = "Hacker",
                role = "ADMIN",
                active = false,
                newPassword = "pwnd"
            });
            putRes.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);

            var delRes = await Client.DeleteAsync("/api/UserProfile/testuser6");
            delRes.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }

        // Matches your controller comment: old token username won't resolve after rename -> force re-login
        [Fact]
        public async Task After_Username_Change_Old_Token_Should_Fail_For_Password_Change()
        {
            var token = await RegisterAndGetTokenAsync(UniqueUsername("testuser7"), password: "test123");
            UseToken(token);

            // rename via /me
            var updateRes = await Client.PutAsJsonAsync("/api/UserProfile/me", new UpdateMyProfileRequest
            {
                Username = "renamed7",
                Name = "Renamed User",
                Email = "renamed7@example.com",
                Phone = "777",
                BirthYear = 1991
            });
            updateRes.StatusCode.Should().Be(HttpStatusCode.OK);

            // old token still contains "testuser7" -> should not find user by token username
            var pwRes = await Client.PutAsJsonAsync("/api/UserProfile/me/password", new ChangePasswordRequest
            {
                CurrentPassword = "test123",
                NewPassword = "newpass"
            });

            pwRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
