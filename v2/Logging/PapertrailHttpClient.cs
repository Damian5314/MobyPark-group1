using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace v2.Logging
{
    public class PapertrailHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public PapertrailHttpClient(string token)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Configure(IConfiguration configuration)
        {
            // No additional configuration needed
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream, CancellationToken cancellationToken = default)
        {
            using var content = new StreamContent(contentStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await _httpClient.PostAsync(requestUri, content, cancellationToken);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
