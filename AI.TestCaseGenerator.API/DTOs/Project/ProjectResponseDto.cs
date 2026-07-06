namespace AI.TestCaseGenerator.API.DTOs.Project
{
    public class ProjectResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int UserId { get; set; }

        public int TotalDocuments { get; set; }

        public int TotalTestCases { get; set; }

        public int TotalChats { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}