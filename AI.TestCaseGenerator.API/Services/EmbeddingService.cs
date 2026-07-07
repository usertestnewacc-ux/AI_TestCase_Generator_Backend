using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

    var response = await _httpClient.PostAsync(
        "https://api.openai.com/v1/embeddings",
        content);

    response.EnsureSuccessStatusCode();

    var responseJson =
        await response.Content.ReadAsStringAsync();

    using JsonDocument doc =
        JsonDocument.Parse(responseJson);

    var embedding =
        doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");

    List<float> vector = new();

    foreach (var item in embedding.EnumerateArray())
    {
        vector.Add(item.GetSingle());
    }

    return vector.ToArray();
}

    }
}