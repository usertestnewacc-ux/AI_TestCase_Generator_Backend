namespace AI.TestCaseGenerator.API.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}