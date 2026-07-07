using AI.TestCaseGenerator.API.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get current logged-in user's profile.
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProfile()
        {
            var profile = new UserProfileDto
            {
                Id = GetCurrentUserId(),
                FullName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(profile);
        }

        /// <summary>
        /// Update current user's profile.
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateProfile([FromBody] object dto)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Profile update is not implemented in the current service layer."
            });
        }

        /// <summary>
        /// Change user password.
        /// </summary>
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ChangePassword([FromBody] object dto)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Password change is not implemented in the current service layer."
            });
        }

        /// <summary>
        /// Delete current user account.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteAccount()
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                Success = false,
                Message = "Account deletion is not implemented in the current service layer."
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