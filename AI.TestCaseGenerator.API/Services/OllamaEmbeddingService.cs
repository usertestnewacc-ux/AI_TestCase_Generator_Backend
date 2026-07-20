using AI.TestCaseGenerator.API.Configuration;
using AI.TestCaseGenerator.API.DTOs.Ollama;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AI.TestCaseGenerator.API.Services
{
    public class OllamaEmbeddingService : IOllamaEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<OllamaSettings> _options;
        private readonly ILogger<OllamaEmbeddingService> _logger;

        public OllamaEmbeddingService(HttpClient httpClient, IOptions<OllamaSettings> options, ILogger<OllamaEmbeddingService> logger)
        {
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text must not be empty.", nameof(text));
            }

            var settings = _options.Value;

            if (!settings.Enabled)
            {
                throw new InvalidOperationException("Ollama embedding generation is disabled in configuration.");
            }

            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                throw new InvalidOperationException("Ollama BaseUrl is not configured.");
            }

            if (string.IsNullOrWhiteSpace(settings.EmbeddingModel))
            {
                throw new InvalidOperationException("Ollama EmbeddingModel is not configured.");
            }

            var request = new OllamaEmbeddingRequest
            {
                Model = settings.EmbeddingModel,
                Prompt = text
            };

            var uri = new Uri(new Uri(settings.BaseUrl.TrimEnd('/') + "/"), "api/embeddings");
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var response = await _httpClient.PostAsync(uri, content, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Ollama embedding request failed with status {(int)response.StatusCode}: {responseBody}");
                }

                var embeddingResponse = System.Text.Json.JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseBody, jsonOptions);

                if (embeddingResponse?.Embedding is null || embeddingResponse.Embedding.Count == 0)
                {
                    throw new InvalidOperationException($"Ollama embedding response was empty or could not be deserialized. Response body: {responseBody}");
                }

                return embeddingResponse.Embedding.ToArray();
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not HttpRequestException)
            {
                _logger.LogError(ex, "Unexpected error while calling Ollama embeddings endpoint.");
                throw new InvalidOperationException("Ollama embedding response could not be deserialized.", ex);
            }
        }
    }
}
