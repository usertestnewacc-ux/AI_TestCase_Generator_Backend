using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AI.TestCaseGenerator.API.DTOs.Document
{
    public class UploadDocumentDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please select a document.")]
        public IFormFile File { get; set; } = null!;
    }
}