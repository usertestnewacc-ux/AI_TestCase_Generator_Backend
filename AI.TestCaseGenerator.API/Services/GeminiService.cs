using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace AI.TestCaseGenerator.API.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
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
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-pro";
            var endpoint = _configuration["Gemini:Endpoint"] ?? "https://generativelanguage.googleapis.com/";

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var request = new
            {
                model = model,
                prompt = prompt,
                temperature = 0.2
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            const int maxRetries = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    try
                    {
                        using var doc = JsonDocument.Parse(responseJson);
                        // Try common shapes: candidates[0].content or output[0].content
                        if (doc.RootElement.TryGetProperty("candidates", out var cand) && cand.GetArrayLength() > 0)
                        {
                            var textEl = cand[0].GetProperty("content");
                            return textEl.GetString() ?? responseJson;
                        }

                        if (doc.RootElement.TryGetProperty("output", out var outEl) && outEl.GetArrayLength() > 0)
                        {
                            var textEl = outEl[0].GetProperty("content");
                            return textEl.GetString() ?? responseJson;
                        }

                        // Fallback: return raw response
                        return responseJson;
                    }
                    catch
                    {
                        return responseJson;
                    }
                }

                if (response.StatusCode == (HttpStatusCode)429 && attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API failed: {(int)response.StatusCode} - {body}");
            }

            throw new HttpRequestException("Gemini API failed after retries.");
        }
    }
}
