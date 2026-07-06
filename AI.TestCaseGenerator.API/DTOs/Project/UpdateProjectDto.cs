using System.ComponentModel.DataAnnotations;

namespace AI.TestCaseGenerator.API.DTOs.Project
{
    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Project name is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}