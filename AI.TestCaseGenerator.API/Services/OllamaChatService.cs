using AI.TestCaseGenerator.API.Configuration;
using AI.TestCaseGenerator.API.DTOs.Ollama;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AI.TestCaseGenerator.API.Services
{
    public class OllamaChatService : IOllamaChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<OllamaSettings> _options;
        private readonly ILogger<OllamaChatService> _logger;

        public OllamaChatService(HttpClient httpClient, IOptions<OllamaSettings> options, ILogger<OllamaChatService> logger)
        {
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public async Task<string> AskAsync(string prompt)
        {
            if (prompt is null)
            {
                throw new ArgumentNullException(nameof(prompt));
            }

            var settings = _options.Value;

            if (!settings.Enabled)
            {
                throw new InvalidOperationException("Ollama is disabled in configuration.");
            }

            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                throw new InvalidOperationException("Ollama BaseUrl is not configured.");
            }

            if (string.IsNullOrWhiteSpace(settings.ChatModel))
            {
                throw new InvalidOperationException("Ollama ChatModel is not configured.");
            }

            var request = new OllamaChatRequest
            {
                Model = settings.ChatModel,
                Messages = new List<OllamaMessage>
                {
                    new OllamaMessage { Role = "user", Content = prompt }
                },
                Stream = false
            };

            var uri = new Uri(new Uri(settings.BaseUrl.TrimEnd('/') + "/"), "api/chat");
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
                var response = await _httpClient.PostAsync(uri, content, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Ollama chat request failed with status {(int)response.StatusCode}: {responseBody}");
                }

                var chatResponse = System.Text.Json.JsonSerializer.Deserialize<OllamaChatResponse>(responseBody, jsonOptions);

                if (chatResponse?.Message is null || string.IsNullOrWhiteSpace(chatResponse.Message.Content))
                {
                    throw new InvalidOperationException($"Ollama chat returned no content. Response body: {responseBody}");
                }

                return chatResponse.Message.Content;
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not HttpRequestException)
            {
                _logger.LogError(ex, "Unexpected error while calling Ollama chat endpoint.");
                throw new InvalidOperationException("Ollama response could not be deserialized.", ex);
            }
        }
    }
}
