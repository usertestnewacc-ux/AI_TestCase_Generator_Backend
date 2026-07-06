namespace AI.TestCaseGenerator.API.DTOs.Document
{
    public class DocumentChunkDto
    {
        public int Id { get; set; }

        public int ChunkIndex { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? EmbeddingId { get; set; }
    }
}