using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using v2.Models;
using Xunit;


namespace v2.Tests
{
    public class UserProfileTests : TestBase
    {
        // ───────────────────────────────────────────────
        // GET PROFILE
        // ───────────────────────────────────────────────
        [Fact]
        public async Task User_Can_Get_Own_Profile()
        {
            var token = await AuthHelper.RegisterAndGetToken(Client);
            UseToken(token);

            var res = await Client.GetAsync("/api/UserProfile/testuser");
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = await res.Content.ReadFromJsonAsync<UserProfile>();
            user!.Username.Should().Be("testuser");
        }

        // ───────────────────────────────────────────────
        // UPDATE PROFILE
        // ───────────────────────────────────────────────
        [Fact]
        public async Task User_Cannot_Change_Role_Or_Active()
        {
            var token = await AuthHelper.RegisterAndGetToken(Client);
            UseToken(token);

            var update = new UserProfile
            {
                Name = "Updated User",
                Email = "updated@example.com",
                Phone = "123456",
                BirthYear = 1990,
                Role = "ADMIN",
                Active = false
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/testuser", update);
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await res.Content.ReadFromJsonAsync<UserProfile>();

            updated!.Role.Should().Be("USER");   // not changed
            updated.Active.Should().BeTrue();    // not changed
        }

        // ───────────────────────────────────────────────
        // PASSWORD CHANGE
        // ───────────────────────────────────────────────
        [Fact]
        public async Task User_Can_Change_Password()
        {
            var token = await AuthHelper.RegisterAndGetToken(Client);
            UseToken(token);

            var req = new ChangePasswordRequest
            {
                CurrentPassword = "test123",
                NewPassword = "newpass"
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/testuser/password", req);
            res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Wrong_CurrentPassword_Should_Fail()
        {
            var token = await AuthHelper.RegisterAndGetToken(Client);
            UseToken(token);

            var req = new ChangePasswordRequest
            {
                CurrentPassword = "WRONG",
                NewPassword = "newpass"
            };

            var res = await Client.PutAsJsonAsync("/api/UserProfile/testuser/password", req);
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ───────────────────────────────────────────────
        // DELETE PROFILE
        // ───────────────────────────────────────────────
        [Fact]
        public async Task User_Can_Delete_Own_Profile()
        {
            var token = await AuthHelper.RegisterAndGetToken(Client);
            UseToken(token);

            var res = await Client.DeleteAsync("/api/UserProfile/testuser");
            res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
