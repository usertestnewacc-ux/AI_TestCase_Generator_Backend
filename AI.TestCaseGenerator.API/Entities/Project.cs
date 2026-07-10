namespace AI.TestCaseGenerator.API.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();

    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
}