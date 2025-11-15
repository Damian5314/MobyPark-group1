using System.Net.Http.Json;
using v2.Models;

namespace v2.Tests
{
    public static class AuthHelper
    {
        public static async Task<string> RegisterAndGetToken(HttpClient client, string username = "testuser")
        {
            var req = new RegisterRequest
            {
                Username = username,
                Password = "test123",
                Name = "Test User",
                Email = "test@example.com"
            };

            var res = await client.PostAsJsonAsync("/register", req);
            res.EnsureSuccessStatusCode();

            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
            return auth!.Token;
        }
    }
}
