namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IOllamaEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
