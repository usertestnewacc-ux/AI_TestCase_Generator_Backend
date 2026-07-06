namespace AI.TestCaseGenerator.API.DTOs.Document
{
    public class ProcessDocumentResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TotalChunks { get; set; }

        public TimeSpan ProcessingTime { get; set; }
    }
}