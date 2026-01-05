using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace v2.Tests
{
    public class TestBase
    {
        protected readonly HttpClient Client;

        public TestBase()
        {
            var factory = new WebApplicationFactory<Program>();
            Client = factory.CreateClient();
        }

        protected void UseToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
