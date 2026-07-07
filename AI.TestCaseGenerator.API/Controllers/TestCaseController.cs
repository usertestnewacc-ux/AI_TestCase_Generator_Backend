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
            var result = await _testCaseService.GenerateTestCasesAsync(dto);

            return Ok(result);
        }

        /// <summary>
        /// Get all test cases for a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        [ProducesResponseType(typeof(IEnumerable<TestCaseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjectTestCases(int projectId)
        {
            var result = await _testCaseService.GetAllAsync(projectId);

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
            var testCase = await _testCaseService.GetByIdAsync(id);

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
            var updated = await _testCaseService.UpdateAsync(id, dto);

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
            var deleted = await _testCaseService.DeleteAsync(id);

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
            var allCases = await _testCaseService.GetAllAsync(projectId);

            var result = allCases.Where(t =>
                string.IsNullOrWhiteSpace(keyword) ||
                (t.Title?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.Preconditions?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.ExpectedResult?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));

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
            var allCases = await _testCaseService.GetAllAsync(projectId);

            var result = allCases.Where(t =>
                (string.IsNullOrWhiteSpace(priority) || string.Equals(t.Priority, priority, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(testType) || string.Equals(t.TestType, testType, StringComparison.OrdinalIgnoreCase)));

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