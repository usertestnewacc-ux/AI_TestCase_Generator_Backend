namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IOllamaChatService
    {
        Task<string> AskAsync(string prompt);
    }
}
