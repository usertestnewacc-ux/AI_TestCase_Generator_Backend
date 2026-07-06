using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(
            IExportService exportService,
            ILogger<ExportController> logger)
        {
            _exportService = exportService;
            _logger = logger;
        }

        /// <summary>
        /// Export all test cases of a project to Excel.
        /// </summary>
        [HttpGet("excel/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToExcel(int projectId)
        {
            int userId = GetCurrentUserId();

            var file = await _exportService.ExportToExcelAsync(projectId, userId);

            if (file == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No test cases found."
                });
            }

            return File(
                file.FileBytes,
                file.ContentType,
                file.FileName);
        }

        /// <summary>
        /// Export all test cases of a project to PDF.
        /// </summary>
        [HttpGet("pdf/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToPdf(int projectId)
        {
            int userId = GetCurrentUserId();

            var file = await _exportService.ExportToPdfAsync(projectId, userId);

            if (file == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No test cases found."
                });
            }

            return File(
                file.FileBytes,
                file.ContentType,
                file.FileName);
        }

        /// <summary>
        /// Export a single test case to PDF.
        /// </summary>
        [HttpGet("pdf/testcase/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportSingleTestCase(int id)
        {
            int userId = GetCurrentUserId();

            var file = await _exportService.ExportSingleTestCasePdfAsync(id, userId);

            if (file == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Test case not found."
                });
            }

            return File(
                file.FileBytes,
                file.ContentType,
                file.FileName);
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