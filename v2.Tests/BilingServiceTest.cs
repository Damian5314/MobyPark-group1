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
    }
}
