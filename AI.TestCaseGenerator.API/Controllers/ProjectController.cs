using AI.TestCaseGenerator.API.DTOs.Project;
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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects of the logged-in user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjects()
        {
            int userId = GetCurrentUserId();

            var projects = await _projectService.GetAllProjectsAsync(userId);

            return Ok(projects);
        }

        /// <summary>
        /// Get project by Id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjectById(int id)
        {
            int userId = GetCurrentUserId();

            var project = await _projectService.GetProjectByIdAsync(id, userId);

            if (project == null)
                return NotFound(new
                {
                    Success = false,
                    Message = "Project not found."
                });

            return Ok(project);
        }

        /// <summary>
        /// Create new project.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            int userId = GetCurrentUserId();

            var project = await _projectService.CreateProjectAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetProjectById),
                new { id = project.Id },
                project);
        }

        /// <summary>
        /// Update project.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
        {
            int userId = GetCurrentUserId();

            var project = await _projectService.UpdateProjectAsync(id, dto, userId);

            if (project == null)
                return NotFound(new
                {
                    Success = false,
                    Message = "Project not found."
                });

            return Ok(project);
        }

        /// <summary>
        /// Delete project.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProject(int id)
        {
            int userId = GetCurrentUserId();

            var deleted = await _projectService.DeleteProjectAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Project not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Project deleted successfully."
            });
        }

        /// <summary>
        /// Returns current logged-in user's Id from JWT.
        /// </summary>
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            return int.Parse(claim.Value);
        }
    }
}