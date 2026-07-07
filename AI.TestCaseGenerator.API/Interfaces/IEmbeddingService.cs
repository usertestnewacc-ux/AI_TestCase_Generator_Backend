namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generate embedding vector for given text.
        /// </summary>
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}