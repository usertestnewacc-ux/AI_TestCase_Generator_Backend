using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace AI.TestCaseGenerator.API.Services
{
    public class ClaudeService : IClaudeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IGeminiService? _geminiService;

        public ClaudeService(
            HttpClient httpClient,
            IConfiguration configuration,
            IGeminiService? geminiService = null)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _geminiService = geminiService;
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

            const int maxRetries = 3;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

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

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(responseJson);

                    return document.RootElement.GetProperty("content")[0].GetProperty("text").GetString()!;
                }

                // If Claude fails (billing, rate limit, auth), attempt OpenAI fallback if configured
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == (HttpStatusCode)429 && attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                    continue;
                }

                // Try Gemini fallback first if enabled
                var geminiEnabled = bool.TryParse(_configuration["Gemini:Enabled"], out var gEnabled) && gEnabled;
                if (geminiEnabled && _geminiService != null)
                {
                    try
                    {
                        var geminiResp = await _geminiService.GenerateResponseAsync(prompt);
                        return geminiResp;
                    }
                    catch (Exception gEx)
                    {
                        responseBody += " | Gemini fallback error: " + gEx.Message;
                    }
                }

                // Try OpenAI fallback
                var openAiKey = _configuration["OpenAI:ApiKey"];
                if (!string.IsNullOrWhiteSpace(openAiKey))
                {
                    try
                    {
                        var openAiResponse = await CallOpenAIAsync(prompt, openAiKey);
                        return openAiResponse;
                    }
                    catch (Exception oaEx)
                    {
                        // fall through to throwing combined error
                        responseBody += " | OpenAI fallback error: " + oaEx.Message;
                    }
                }

                throw new HttpRequestException($"Claude API failed: {(int)response.StatusCode} - {responseBody}");
            }

            throw new HttpRequestException("Claude API failed after retries.");
        }

        private async Task<string> CallOpenAIAsync(string prompt, string apiKey)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var model = _configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";

            var request = new
            {
                model = model,
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.2
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var message))
                {
                    if (message.TryGetProperty("content", out var contentEl))
                    {
                        return contentEl.GetString() ?? string.Empty;
                    }
                }

                if (first.TryGetProperty("text", out var textEl))
                {
                    return textEl.GetString() ?? string.Empty;
                }
            }

            throw new HttpRequestException("OpenAI response format was unexpected.");
        }
    }
}