namespace AI.TestCaseGenerator.API.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}