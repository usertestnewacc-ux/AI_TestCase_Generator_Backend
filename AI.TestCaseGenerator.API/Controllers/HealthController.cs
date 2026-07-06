using Microsoft.AspNetCore.Mvc;

namespace AI.TestCaseGenerator.API.Controllers
{
    /// <summary>
    /// Health check endpoints for verifying API availability.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Checks whether the API is running.
        /// </summary>
        /// <returns>Application health status.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Application = "AI Test Case Generator & QA Assistant API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ServerTime = DateTime.UtcNow,
                Message = "API is running successfully."
            });
        }
    }
}