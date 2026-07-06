namespace AI.TestCaseGenerator.API.DTOs.AIChat
{
    public class ChatHistoryDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}