namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IChromaDbService
    {
        Task CreateCollectionAsync(string collectionName);

        Task AddEmbeddingAsync(
            string collectionName,
            string id,
            float[] embedding,
            string document);

        Task<List<string>> SearchAsync(
            string collectionName,
            float[] embedding,
            int topK = 5);

        Task DeleteEmbeddingAsync(
            string collectionName,
            string id);
    }
}