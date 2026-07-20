using AI.TestCaseGenerator.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.TestCaseGenerator.API.Controllers
{
    [ApiController]
    [Route("api/ollama/embedding")]
    public class OllamaEmbeddingTestController : ControllerBase
    {
        private readonly IOllamaEmbeddingService _ollamaEmbeddingService;

        public OllamaEmbeddingTestController(IOllamaEmbeddingService ollamaEmbeddingService)
        {
            _ollamaEmbeddingService = ollamaEmbeddingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmbedding([FromBody] OllamaEmbeddingRequestDto request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { message = "Text is required." });
            }

            try
            {
                var embedding = await _ollamaEmbeddingService.GenerateEmbeddingAsync(request.Text);
                var result = new
                {
                    dimensions = embedding.Length,
                    embedding = embedding.Take(10).Concat(embedding.Skip(Math.Max(0, embedding.Length - 10))).ToArray()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }
    }

    public class OllamaEmbeddingRequestDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
