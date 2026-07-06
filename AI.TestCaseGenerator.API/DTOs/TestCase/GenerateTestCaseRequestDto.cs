using System.ComponentModel.DataAnnotations;

namespace AI.TestCaseGenerator.API.DTOs.TestCase
{
    public class GenerateTestCaseRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        /// <summary>
        /// Optional specific module.
        /// Example: Login, Payment
        /// </summary>
        public string? ModuleName { get; set; }

        /// <summary>
        /// Positive / Negative / Edge / Regression / All
        /// </summary>
        public string TestType { get; set; } = "All";

        /// <summary>
        /// Number of test cases to generate.
        /// </summary>
        [Range(1, 100)]
        public int NumberOfTestCases { get; set; } = 10;

        /// <summary>
        /// Additional instructions for Claude.
        /// </summary>
        public string? Prompt { get; set; }
    }
}