using AI.TestCaseGenerator.API.DTOs.User;
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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current logged-in user's profile.
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetCurrentUserId();

            var user = await _userService.GetProfileAsync(userId);

            if (user == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            return Ok(user);
        }

        /// <summary>
        /// Update current user's profile.
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateUserDto dto)
        {
            int userId = GetCurrentUserId();

            var user = await _userService.UpdateProfileAsync(userId, dto);

            if (user == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Unable to update profile."
                });
            }

            return Ok(user);
        }

        /// <summary>
        /// Change user password.
        /// </summary>
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            int userId = GetCurrentUserId();

            var changed = await _userService.ChangePasswordAsync(userId, dto);

            if (!changed)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Current password is incorrect."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Password changed successfully."
            });
        }

        /// <summary>
        /// Delete current user account.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount()
        {
            int userId = GetCurrentUserId();

            var deleted = await _userService.DeleteAccountAsync(userId);

            if (!deleted)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Unable to delete account."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Account deleted successfully."
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