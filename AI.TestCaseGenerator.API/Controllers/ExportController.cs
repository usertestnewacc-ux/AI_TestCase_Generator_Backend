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
        private readonly ILogger<ExportController> _logger;

        public ExportController(ILogger<ExportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Export all test cases of a project to Excel.
        /// </summary>
        [HttpGet("excel/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ExportToExcel(int projectId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Export functionality is not available in the current service implementation."
            });
        }

        /// <summary>
        /// Export all test cases of a project to PDF.
        /// </summary>
        [HttpGet("pdf/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ExportToPdf(int projectId)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Export functionality is not available in the current service implementation."
            });
        }

        /// <summary>
        /// Export a single test case to PDF.
        /// </summary>
        [HttpGet("pdf/testcase/{id:int}")]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult ExportSingleTestCase(int id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Export functionality is not available in the current service implementation."
            });
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