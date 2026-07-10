namespace AI.TestCaseGenerator.API.Entities;

public class DocumentChunk : BaseEntity
{
    public int DocumentId { get; set; }

    public Document Document { get; set; } = null!;

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? EmbeddingId { get; set; }
}