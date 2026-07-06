using AI.TestCaseGenerator.API.DTOs.User;

namespace AI.TestCaseGenerator.API.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime Expiration { get; set; }

        public UserProfileDto? User { get; set; }

        public string? RefreshToken { get; set; }
    }
}