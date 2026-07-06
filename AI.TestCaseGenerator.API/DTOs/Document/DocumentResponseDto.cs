namespace AI.TestCaseGenerator.API.DTOs.Document
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        // RAG Information
        public int TotalChunks { get; set; }

        public bool IsProcessed { get; set; }
    }
}