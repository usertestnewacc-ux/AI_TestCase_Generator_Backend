using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;

namespace AI.TestCaseGenerator.API.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            var model = _configuration["OpenAI:EmbeddingModel"];

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                input = text,
                model = model
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            const int maxRetries = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/embeddings",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(responseJson);

                    var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");

                    List<float> vector = new();

                    foreach (var item in embedding.EnumerateArray())
                    {
                        vector.Add(item.GetSingle());
                    }

                    return vector.ToArray();
                }

                if (response.StatusCode == (HttpStatusCode)429 && attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Embedding API failed: {(int)response.StatusCode} - {body}");
            }

            throw new HttpRequestException("Embedding API failed after retries.");
        }

    }
}