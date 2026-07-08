using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AI.TestCaseGenerator.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            ApplicationDbContext context,
            ILogger<UserController> logger)
        {
            _context = context;
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
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Change user password.
        /// </summary>
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            {
                return BadRequest(new { Success = false, Message = "All password fields are required." });
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { Success = false, Message = "New password and confirmation do not match." });
            }

            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            if (!BCryptNet.Verify(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { Success = false, Message = "Current password is incorrect." });

            user.PasswordHash = BCryptNet.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Password changed successfully." });
        }

        /// <summary>
        /// Delete current user account.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Account deleted successfully." });
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