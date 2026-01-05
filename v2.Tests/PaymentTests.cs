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
    public class PaymentTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PaymentTests(WebApplicationFactory<Program> factory)
        {
            var f = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var paymentDescriptors = services
                        .Where(d => d.ServiceType == typeof(IPaymentService))
                        .ToList();

                    foreach (var d in paymentDescriptors)
                        services.Remove(d);

                    services.AddScoped<IPaymentService, PaymentService>();
                });
            });

            _client = f.CreateClient();
        }

        [Fact]
        public async Task GetAll_Should_Return_List_Of_Payments()
        {
            var response = await _client.GetAsync("/api/Payment");

            response.EnsureSuccessStatusCode();
            var payments = await response.Content.ReadFromJsonAsync<List<Payment>>();

            payments.Should().NotBeNull();
            payments.Should().BeOfType<List<Payment>>();
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_For_Nonexistent_Payment()
        {
            var response = await _client.GetAsync("/api/Payment/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByInitiator_Should_Return_Unauthorized_Without_Admin_Token()
        {
            var response = await _client.GetAsync("/api/Payment/initiator/testuser");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUnpaidSessions_Should_Return_List_Of_Sessions()
        {
            var response = await _client.GetAsync("/api/Payment/unpaid/XX-YY-99");

            response.EnsureSuccessStatusCode();
            var sessions = await response.Content.ReadFromJsonAsync<List<ParkingSession>>();

            sessions.Should().NotBeNull();
            sessions.Should().BeOfType<List<ParkingSession>>();
        }

        [Fact]
        public async Task Delete_Should_Return_Unauthorized_Without_Admin_Token()
        {
            var response = await _client.DeleteAsync("/api/Payment/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetByInitiator_Should_Return_Payments_With_Token()
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = "payment_initiator_user",
                Password = "test123",
                Name = "Payment Initiator User",
                Email = "paymentinitiator@test.com",
                Phone = "+31612345662",
                BirthYear = 1990
            });

            registerResponse.EnsureSuccessStatusCode();
            var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authData!.Token);

            var response = await _client.GetAsync("/api/Payment/initiator/testuser");

            response.EnsureSuccessStatusCode();
            var payments = await response.Content.ReadFromJsonAsync<List<Payment>>();

            payments.Should().NotBeNull();
            payments.Should().BeOfType<List<Payment>>();
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_For_Nonexistent_Payment()
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterRequest
            {
                Username = "payment_delete_user",
                Password = "test123",
                Name = "Payment Delete User",
                Email = "paymentdelete@test.com",
                Phone = "+31612345663",
                BirthYear = 1990
            });

            registerResponse.EnsureSuccessStatusCode();
            var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authData!.Token);

            var response = await _client.DeleteAsync("/api/Payment/99999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
