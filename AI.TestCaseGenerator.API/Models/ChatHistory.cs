namespace AI.TestCaseGenerator.API.Entities;

public class ChatHistory : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string UserQuestion { get; set; } = string.Empty;

    public string AiResponse { get; set; } = string.Empty;
}