namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IClaudeService
    {
        /// <summary>
        /// Sends a prompt to Claude and returns the generated response.
        /// </summary>
        Task<string> GenerateResponseAsync(string prompt);

        /// <summary>
        /// Generates software test cases using Claude.
        /// </summary>
        Task<string> GenerateTestCasesAsync(string prompt);
    }
}