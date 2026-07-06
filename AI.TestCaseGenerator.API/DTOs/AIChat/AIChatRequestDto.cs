using System.ComponentModel.DataAnnotations;

namespace AI.TestCaseGenerator.API.DTOs.AIChat
{
    public class AIChatRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(5000)]
        public string Question { get; set; } = string.Empty;
    }
}