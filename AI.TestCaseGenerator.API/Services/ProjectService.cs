using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.Project;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AI.TestCaseGenerator.API.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectService(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(int userId)
{
    var projects = await _context.Projects
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

    return _mapper.Map<IEnumerable<ProjectResponseDto>>(projects);
}

public async Task<ProjectResponseDto?> GetProjectByIdAsync(int id, int userId)
{
    var project = await _context.Projects
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.UserId == userId);

    if (project == null)
        return null;

    return _mapper.Map<ProjectResponseDto>(project);
}

public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, int userId)
{
    var project = new Project
    {
        Name = dto.Name,
        Description = dto.Description,
        UserId = userId
    };

    _context.Projects.Add(project);

    await _context.SaveChangesAsync();

    return _mapper.Map<ProjectResponseDto>(project);
}

public async Task<ProjectResponseDto?> UpdateProjectAsync(
    int id,
    UpdateProjectDto dto,
    int userId)
{
    var project = await _context.Projects
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.UserId == userId);

    if (project == null)
        return null;

    project.Name = dto.Name;
    project.Description = dto.Description;

    await _context.SaveChangesAsync();

    return _mapper.Map<ProjectResponseDto>(project);
}

public async Task<bool> DeleteProjectAsync(int id, int userId)
{
    var project = await _context.Projects
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.UserId == userId);

    if (project == null)
        return false;

    _context.Projects.Remove(project);

    await _context.SaveChangesAsync();

    return true;
}


    }



    

}