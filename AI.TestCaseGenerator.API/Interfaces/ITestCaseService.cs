using AI.TestCaseGenerator.API.DTOs.TestCase;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface ITestCaseService
    {
        Task<IEnumerable<TestCaseResponseDto>> GetAllAsync(int projectId);

        Task<TestCaseResponseDto?> GetByIdAsync(int id);

        Task<IEnumerable<TestCaseResponseDto>> GenerateTestCasesAsync(
            GenerateTestCaseRequestDto request);

        Task<TestCaseResponseDto?> UpdateAsync(
            int id,
            UpdateTestCaseDto dto);

        Task<bool> DeleteAsync(int id);
    }
}