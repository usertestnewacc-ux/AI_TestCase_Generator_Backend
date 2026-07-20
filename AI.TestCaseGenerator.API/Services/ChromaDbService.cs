using System.Text;
using System.Text.Json;
using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AI.TestCaseGenerator.API.Services
{
    public class ChromaDbService : IChromaDbService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChromaDbService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        private string BaseUrl =>
            _configuration["ChromaDB:BaseUrl"]!;

        private string Tenant =>
            _configuration["ChromaDB:Tenant"] ?? "default_tenant";

        private string Database =>
            _configuration["ChromaDB:Database"] ?? "default_database";

        private string CollectionsBasePath =>
            $"{BaseUrl}/api/v2/tenants/{Tenant}/databases/{Database}/collections";

        private async Task<string?> GetCollectionIdByNameAsync(string collectionName)
        {
            var response = await _httpClient.GetAsync(CollectionsBasePath);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.GetProperty("name").GetString() == collectionName)
                {
                    return item.GetProperty("id").GetString();
                }
            }

            return null;
        }

        private async Task<string> GetOrCreateCollectionIdAsync(string collectionName)
        {
            var collectionId = await GetCollectionIdByNameAsync(collectionName);
            if (!string.IsNullOrWhiteSpace(collectionId))
                return collectionId;

            var body = new
            {
                name = collectionName,
                get_or_create = true
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                CollectionsBasePath,
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            return document.RootElement.GetProperty("id").GetString()!;
        }

        public async Task CreateCollectionAsync(string collectionName)
        {
            await GetOrCreateCollectionIdAsync(collectionName);
        }

        public async Task AddEmbeddingAsync(
            string collectionName,
            string id,
            float[] embedding,
            string document)
        {
            var collectionId = await GetOrCreateCollectionIdAsync(collectionName);

            var body = new
            {
                ids = new[] { id },
                embeddings = new[]
                {
                    embedding
                },
                documents = new[]
                {
                    document
                }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                $"{CollectionsBasePath}/{collectionId}/add",
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<string>> SearchAsync(
            string collectionName,
            float[] embedding,
            int topK = 5)
        {
            var collectionId = await GetOrCreateCollectionIdAsync(collectionName);

            var body = new
            {
                query_embeddings = new[]
                {
                    embedding
                },
                n_results = topK
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                $"{CollectionsBasePath}/{collectionId}/query",
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();

            var responseJson =
                await response.Content.ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(responseJson);

            var results = new List<string>();

            if (!document.RootElement.TryGetProperty("documents", out var documentsElement) ||
                documentsElement.GetArrayLength() == 0)
            {
                return results;
            }

            var docs = documentsElement[0];

            foreach (var item in docs.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    results.Add(value);
                }
            }

            return results;
        }

        public async Task DeleteEmbeddingAsync(
            string collectionName,
            string id)
        {
            var collectionId = await GetOrCreateCollectionIdAsync(collectionName);

            var body = new
            {
                ids = new[]
                {
                    id
                }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                $"{CollectionsBasePath}/{collectionId}/delete",
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();
        }

    }
}