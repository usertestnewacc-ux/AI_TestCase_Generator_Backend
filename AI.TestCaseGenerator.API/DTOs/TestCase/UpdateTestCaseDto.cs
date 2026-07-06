using System.ComponentModel.DataAnnotations;

namespace AI.TestCaseGenerator.API.DTOs.TestCase
{
    public class UpdateTestCaseDto
    {
        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Preconditions { get; set; } = string.Empty;

        [Required]
        public string TestSteps { get; set; } = string.Empty;

        [Required]
        public string ExpectedResult { get; set; } = string.Empty;

        [Required]
        public string TestType { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium";
    }
}