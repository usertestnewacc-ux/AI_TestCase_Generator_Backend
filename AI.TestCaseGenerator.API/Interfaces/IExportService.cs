using AI.TestCaseGenerator.API.DTOs.Export;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IExportService
    {
        Task<ExportFileDto> ExportProjectTestCasesToExcelAsync(int projectId, int userId);
        Task<ExportFileDto> ExportProjectTestCasesToPdfAsync(int projectId, int userId);
    }
}
