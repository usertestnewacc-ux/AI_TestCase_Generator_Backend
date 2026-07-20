using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.TestCaseGenerator.API.Controllers
{
    [ApiController]
    [Route("api/ollama")]
    public class OllamaTestController : ControllerBase
    {
        private readonly IOllamaChatService _ollamaChatService;

        public OllamaTestController(IOllamaChatService ollamaChatService)
        {
            _ollamaChatService = ollamaChatService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] OllamaChatRequestDto request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "Prompt is required." });
            }

            try
            {
                var response = await _ollamaChatService.AskAsync(request.Prompt);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }
    }

    public class OllamaChatRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
