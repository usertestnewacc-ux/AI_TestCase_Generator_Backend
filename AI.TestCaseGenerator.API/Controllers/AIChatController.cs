using AI.TestCaseGenerator.API.DTOs.AIChat;
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
    public class AIChatController : ControllerBase
    {
        private readonly IAIChatService _aiChatService;
        private readonly ILogger<AIChatController> _logger;

        public AIChatController(
            IAIChatService aiChatService,
            ILogger<AIChatController> logger)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        /// <summary>
        /// Ask AI a question about a project.
        /// </summary>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AskQuestion([FromBody] AIChatRequestDto dto)
        {
            int userId = GetCurrentUserId();

            var response = await _aiChatService.AskQuestionAsync(dto, userId);

            return Ok(response);
        }

        /// <summary>
        /// Get chat history for a project.
        /// </summary>
        [HttpGet("history/{projectId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ChatHistoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatHistory(int projectId)
        {
            int userId = GetCurrentUserId();

            var history = await _aiChatService.GetChatHistoryAsync(projectId, userId);

            return Ok(history);
        }

        /// <summary>
        /// Delete chat history of a project.
        /// </summary>
        [HttpDelete("history/{projectId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteChatHistory(int projectId)
        {
            int userId = GetCurrentUserId();

            var deleted = await _aiChatService.DeleteChatHistoryAsync(projectId, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "No chat history found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Chat history deleted successfully."
            });
        }

        /// <summary>
        /// Regenerate previous AI response.
        /// </summary>
        [HttpPost("regenerate")]
        [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegenerateResponse([FromBody] AIChatRequestDto dto)
        {
            int userId = GetCurrentUserId();

            var response = await _aiChatService.RegenerateResponseAsync(dto, userId);

            return Ok(response);
        }

        /// <summary>
        /// Returns current logged-in user's Id.
        /// </summary>
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            return int.Parse(claim.Value);
        }
    }
}