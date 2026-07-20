using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AI.TestCaseGenerator.API.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model = _configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";

            var requestBody = new
            {
                model,
                input = text
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/api/embeddings", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ollama embedding request failed: {(int)response.StatusCode} - {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("embedding", out var embeddingElement))
            {
                var vector = new List<float>();

                foreach (var item in embeddingElement.EnumerateArray())
                {
                    vector.Add(item.TryGetSingle(out var singleValue) ? singleValue : (float)item.GetDouble());
                }

                return vector.ToArray();
            }

            throw new HttpRequestException("Ollama embedding response was unexpected.");
        }
    }
}