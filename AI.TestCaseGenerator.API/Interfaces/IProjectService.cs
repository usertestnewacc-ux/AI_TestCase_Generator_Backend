using AI.TestCaseGenerator.API.DTOs.Project;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(int userId);

        Task<ProjectResponseDto?> GetProjectByIdAsync(int id, int userId);

        Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, int userId);

        Task<ProjectResponseDto?> UpdateProjectAsync(int id, UpdateProjectDto dto, int userId);

        Task<bool> DeleteProjectAsync(int id, int userId);
    }
}