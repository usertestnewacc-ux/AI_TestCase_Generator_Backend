using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("excel/{projectId:int}")]
        public async Task<IActionResult> ExportExcel(int projectId)
        {
            var userId = GetCurrentUserId();
            var file = await _exportService.ExportProjectTestCasesToExcelAsync(projectId, userId);
            return File(file.FileBytes, file.ContentType, file.FileName);
        }

        [HttpGet("pdf/{projectId:int}")]
        public async Task<IActionResult> ExportPdf(int projectId)
        {
            var userId = GetCurrentUserId();
            var file = await _exportService.ExportProjectTestCasesToPdfAsync(projectId, userId);
            return File(file.FileBytes, file.ContentType, file.FileName);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("User is not authenticated.");
            return int.Parse(claim.Value);
        }
    }
}
