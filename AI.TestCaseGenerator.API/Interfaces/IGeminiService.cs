using System.Threading.Tasks;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GenerateResponseAsync(string prompt);

        Task<string> GenerateTestCasesAsync(string prompt);
    }
}
