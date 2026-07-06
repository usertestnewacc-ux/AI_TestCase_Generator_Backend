namespace AI.TestCaseGenerator.API.DTOs.TestCase
{
    public class TestCaseResponseDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string ModuleName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Preconditions { get; set; } = string.Empty;

        public string TestSteps { get; set; } = string.Empty;

        public string ExpectedResult { get; set; } = string.Empty;

        public string TestType { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public bool IsAiGenerated { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}