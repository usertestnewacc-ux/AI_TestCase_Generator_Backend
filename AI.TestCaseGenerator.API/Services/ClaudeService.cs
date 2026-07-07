using System.Net.Http.Headers;
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

        public ClaudeService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            return await SendPromptAsync(prompt);
        }

        public async Task<string> GenerateTestCasesAsync(string prompt)
        {
            return await SendPromptAsync(prompt);
        }

        private async Task<string> SendPromptAsync(string prompt)
        {
            var apiKey = _configuration["Claude:ApiKey"];
            var model = _configuration["Claude:Model"];

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Add(
                "x-api-key",
                apiKey);

            _httpClient.DefaultRequestHeaders.Add(
                "anthropic-version",
                "2023-06-01");

            var requestBody = new
            {
                model = model,
                max_tokens = 4096,
                temperature = 0.2,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.anthropic.com/v1/messages",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument document =
                JsonDocument.Parse(responseJson);

            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString()!;
        }
    }
}