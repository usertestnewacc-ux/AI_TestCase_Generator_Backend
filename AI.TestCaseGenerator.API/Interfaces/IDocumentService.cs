using AI.TestCaseGenerator.API.DTOs.Document;
using Microsoft.AspNetCore.Http;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(
            UploadDocumentDto dto,
            IFormFile file,
            int userId);

        Task<IEnumerable<DocumentResponseDto>> GetProjectDocumentsAsync(
            int projectId,
            int userId);

        Task<DocumentResponseDto?> GetDocumentByIdAsync(
            int documentId,
            int userId);

        Task<DocumentDownloadDto?> DownloadDocumentAsync(
            int documentId,
            int userId);

        Task<bool> DeleteDocumentAsync(
            int documentId,
            int userId);

        Task<ProcessDocumentResponseDto> ProcessDocumentAsync(
            int documentId);
    }
}