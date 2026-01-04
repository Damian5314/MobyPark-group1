using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class AuthRegisterRealDbTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthRegisterRealDbTests(WebApplicationFactory<Program> factory)
        {
            var f = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // FIX DI LIFETIME ONLY
                    var authDescriptors = services
                        .Where(d => d.ServiceType == typeof(IAuthService))
                        .ToList();

                    foreach (var d in authDescriptors)
                        services.Remove(d);

                    services.AddScoped<IAuthService, AuthService>();
                });
            });

            _client = f.CreateClient();
        }

        private class RegisterResponse
        {
            public string Message { get; set; } = "";
            public string Token { get; set; } = "";
        }

        private class LoginResponse
        {
            public string Message { get; set; } = "";
            public string Token { get; set; } = "";
        }

        private class LogoutResponse
        {
            public string Message { get; set; } = "";
        }


        [Fact]
        public async Task Register()
        {
            var username = "testuser";
            var password = "testpassword";

            var registerRequest = new RegisterRequest
            {
                Username = username,
                Password = password,
                Name = "Real User",
                Email = "testuser@test.com",
                Phone = "+310000001",
                BirthYear = 1995
            };

            var registerRes = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
            registerRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var registerBody = await registerRes.Content.ReadFromJsonAsync<RegisterResponse>();
            registerBody!.Token.Should().NotBeNullOrWhiteSpace();

        }        

        [Fact]
        public async Task Login_Should_Work_With_Registered_Credentials()
        {
            // Arrange (ensure user exists)
            var username = "testuser";
            var password = "testpassword";


            // Act
            var loginRes = await _client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
            {
                Username = username,
                Password = password
            });

            // Assert
            loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginBody = await loginRes.Content.ReadFromJsonAsync<LoginResponse>();
            loginBody.Should().NotBeNull();
            loginBody!.Message.Should().Be("Logged in successfully.");
            loginBody.Token.Should().NotBeNullOrWhiteSpace();
        }
    }
}
