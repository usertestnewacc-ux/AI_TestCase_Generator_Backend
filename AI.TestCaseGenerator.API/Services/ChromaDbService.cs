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


        public async Task CreateCollectionAsync(string collectionName)
{
    var body = new
    {
        name = collectionName
    };

    var json = JsonSerializer.Serialize(body);

    var response = await _httpClient.PostAsync(
        $"{BaseUrl}/api/v1/collections",
        new StringContent(
            json,
            Encoding.UTF8,
            "application/json"));

    response.EnsureSuccessStatusCode();
    }

public async Task AddEmbeddingAsync(
    string collectionName,
    string id,
    float[] embedding,
    string document)
{
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
        $"{BaseUrl}/api/v1/collections/{collectionName}/add",
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
        $"{BaseUrl}/api/v1/collections/{collectionName}/query",
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

    var docs = document.RootElement
        .GetProperty("documents")[0];

    foreach (var item in docs.EnumerateArray())
    {
        results.Add(item.GetString()!);
    }

    return results;
}

public async Task DeleteEmbeddingAsync(
    string collectionName,
    string id)
{
    var body = new
    {
        ids = new[]
        {
            id
        }
    };

    var json = JsonSerializer.Serialize(body);

    var response = await _httpClient.PostAsync(
        $"{BaseUrl}/api/v1/collections/{collectionName}/delete",
        new StringContent(
            json,
            Encoding.UTF8,
            "application/json"));

    response.EnsureSuccessStatusCode();
}

    }
}