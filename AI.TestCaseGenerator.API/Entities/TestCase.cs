namespace AI.TestCaseGenerator.API.Entities;

public class TestCase : BaseEntity
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string ModuleName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Preconditions { get; set; } = string.Empty;

    public string TestSteps { get; set; } = string.Empty;

    public string ExpectedResult { get; set; } = string.Empty;

    public string TestType { get; set; } = string.Empty;

    public string Priority { get; set; } = "Medium";

    public bool IsAiGenerated { get; set; } = true;
}