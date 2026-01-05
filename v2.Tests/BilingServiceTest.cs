using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using v2.Models;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class BillingTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BillingTests(WebApplicationFactory<Program> factory)
        {
            var f = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var billingDescriptors = services
                        .Where(d => d.ServiceType == typeof(IBillingService))
                        .ToList();

                    foreach (var d in billingDescriptors)
                        services.Remove(d);

                    services.AddScoped<IBillingService, BillingService>();
                });
            });

            _client = f.CreateClient();
        }

        [Fact]
        public async Task GetAll_Should_Return_Unauthorized_Without_Admin_Token()
        {
            var response = await _client.GetAsync("/api/Billing");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_Should_Return_OK_With_Admin_Token()
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = "admin_billing_test",
                Password = "admin123",
                Name = "Admin User",
                Email = "admin@test.com",
                Phone = "+31612345678",
                BirthYear = 1990
            });

            var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authData!.Token);

            var response = await _client.GetAsync("/api/Billing");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_Unauthorized_Without_Token()
        {
            var response = await _client.GetAsync("/api/Billing/user/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_NotFound_For_Nonexistent_User()
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = "user_for_404_test",
                Password = "test123",
                Name = "Test User",
                Email = "404test@test.com",
                Phone = "+31612345679",
                BirthYear = 1990
            });

            var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authData!.Token);

            var response = await _client.GetAsync("/api/Billing/user/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByUserId_Should_Return_NotFound_For_User_Without_Payments()
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = "user_no_payments",
                Password = "test123",
                Name = "User Without Payments",
                Email = "nopayments@test.com",
                Phone = "+31612345660",
                BirthYear = 1990
            });

            registerResponse.EnsureSuccessStatusCode();
            var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authData!.Token);

            var getUserResponse = await _client.GetAsync("/api/UserProfile/me");
            getUserResponse.EnsureSuccessStatusCode();
            var userData = await getUserResponse.Content.ReadFromJsonAsync<UserProfile>();

            var response = await _client.GetAsync($"/api/Billing/user/{userData!.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
