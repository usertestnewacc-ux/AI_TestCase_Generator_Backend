using AI.TestCaseGenerator.API.DTOs.TestCase;
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
    public class TestCaseController : ControllerBase
    {
        private readonly ITestCaseService _testCaseService;
        private readonly ILogger<TestCaseController> _logger;

        public TestCaseController(
            ITestCaseService testCaseService,
            ILogger<TestCaseController> logger)
        {
            _testCaseService = testCaseService;
            _logger = logger;
        }

        /// <summary>
        /// Generate AI test cases for a project.
        /// </summary>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(IEnumerable<TestCaseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateTestCases(
            [FromBody] GenerateTestCaseRequestDto dto)
        {
            int userId = GetCurrentUserId();

            var result = await _testCaseService.GenerateTestCasesAsync(dto, userId);

            return Ok(result);
        }

        /// <summary>
        /// Get all test cases for a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        [ProducesResponseType(typeof(IEnumerable<TestCaseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjectTestCases(int projectId)
        {
            int userId = GetCurrentUserId();

            var result = await _testCaseService.GetProjectTestCasesAsync(projectId, userId);

            return Ok(result);
        }

        /// <summary>
        /// Get test case by Id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TestCaseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTestCaseById(int id)
        {
            int userId = GetCurrentUserId();

            var testCase = await _testCaseService.GetTestCaseByIdAsync(id, userId);

            if (testCase == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Test case not found."
                });
            }

            return Ok(testCase);
        }

        /// <summary>
        /// Update a test case.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TestCaseResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTestCase(
            int id,
            [FromBody] UpdateTestCaseDto dto)
        {
            int userId = GetCurrentUserId();

            var updated = await _testCaseService.UpdateTestCaseAsync(id, dto, userId);

            if (updated == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Test case not found."
                });
            }

            return Ok(updated);
        }

        /// <summary>
        /// Delete a test case.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteTestCase(int id)
        {
            int userId = GetCurrentUserId();

            var deleted = await _testCaseService.DeleteTestCaseAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Test case not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Test case deleted successfully."
            });
        }

        /// <summary>
        /// Search test cases.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<TestCaseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchTestCases(
            [FromQuery] int projectId,
            [FromQuery] string keyword)
        {
            int userId = GetCurrentUserId();

            var result = await _testCaseService.SearchTestCasesAsync(projectId, keyword, userId);

            return Ok(result);
        }

        /// <summary>
        /// Filter test cases.
        /// </summary>
        [HttpGet("filter")]
        [ProducesResponseType(typeof(IEnumerable<TestCaseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FilterTestCases(
            [FromQuery] int projectId,
            [FromQuery] string? priority,
            [FromQuery] string? testType)
        {
            int userId = GetCurrentUserId();

            var result = await _testCaseService.FilterTestCasesAsync(
                projectId,
                priority,
                testType,
                userId);

            return Ok(result);
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