using AI.TestCaseGenerator.API.DTOs.Auth;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// Login existing user.
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Refresh JWT token.
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Logout user.
        /// </summary>
        Task<bool> LogoutAsync(int userId);
    }
}