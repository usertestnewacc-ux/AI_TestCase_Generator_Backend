namespace AI.TestCaseGenerator.API.DTOs.AIChat
{
    public class AIChatResponseDto
    {
        public bool Success { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Claude model used.
        /// Example: claude-sonnet-4
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Number of document chunks retrieved from ChromaDB.
        /// </summary>
        public int RetrievedChunks { get; set; }

        /// <summary>
        /// AI response generation time in milliseconds.
        /// </summary>
        public long ResponseTimeMs { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}