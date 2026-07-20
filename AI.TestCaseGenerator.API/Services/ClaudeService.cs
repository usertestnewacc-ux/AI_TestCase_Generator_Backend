using System.Text;
using System.Text.Json;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AI.TestCaseGenerator.API.Services
{
    public class ClaudeService : IClaudeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ClaudeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public Task<string> GenerateResponseAsync(string prompt)
        {
            return SendPromptAsync(prompt);
        }

        public Task<string> GenerateTestCasesAsync(string prompt)
        {
            return SendPromptAsync(prompt);
        }

        private async Task<string> SendPromptAsync(string prompt)
        {
            var baseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model = _configuration["Ollama:ChatModel"] ?? "llama3.2";
            var temperature = 0.2;

            var requestBody = new
            {
                model,
                prompt,
                stream = false,
                options = new { temperature }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/api/generate", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Ollama chat request failed: {(int)response.StatusCode} - {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("response", out var responseElement))
            {
                return responseElement.GetString() ?? string.Empty;
            }

            return responseBody;
        }
    }
}