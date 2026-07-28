using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.TestCase;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AI.TestCaseGenerator.API.Services
{
    public class TestCaseService : ITestCaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOllamaChatService _ollamaChatService;
        private readonly IOllamaEmbeddingService _embeddingService;
        private readonly IChromaDbService _chromaDbService;
        private readonly ILogger<TestCaseService> _logger;

        public TestCaseService(
            ApplicationDbContext context,
            IMapper mapper,
            IOllamaChatService ollamaChatService,
            IOllamaEmbeddingService embeddingService,
            IChromaDbService chromaDbService,
            ILogger<TestCaseService> logger)
        {
            _context = context;
            _mapper = mapper;
            _ollamaChatService = ollamaChatService;
            _embeddingService = embeddingService;
            _chromaDbService = chromaDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<TestCaseResponseDto>> GetAllAsync(int projectId)
{
    var testCases = await _context.TestCases
        .Where(t => t.ProjectId == projectId)
        .OrderBy(t => t.Id)
        .ToListAsync();

    return _mapper.Map<IEnumerable<TestCaseResponseDto>>(testCases);
}

public async Task<TestCaseResponseDto?> GetByIdAsync(int id)
{
    var testCase = await _context.TestCases
        .FirstOrDefaultAsync(t => t.Id == id);

    if (testCase == null)
        return null;

    return _mapper.Map<TestCaseResponseDto>(testCase);
}


public async Task<IEnumerable<TestCaseResponseDto>> GenerateTestCasesAsync(
    GenerateTestCaseRequestDto request)
{

    

    if (string.IsNullOrWhiteSpace(request.Prompt))
        throw new ArgumentException("Prompt is required.", nameof(request.Prompt));

    // Verify project exists
    var project = await _context.Projects
        .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

    if (project == null)
        throw new Exception("Project not found.");

    var moduleName = NormalizeModuleName(request.ModuleName, request.Prompt);

    // Retrieve relevant context from vector search if available; otherwise fall back to stored document chunks.
    var relevantChunks = await GetRelevantChunksAsync(project.Id, request.Prompt);

    // Build RAG prompt
    var prompt = BuildPrompt(request.Prompt, relevantChunks, moduleName);

    // Send to Ollama
    var aiResponse = await _ollamaChatService.AskAsync(prompt);

    // Parse AI response
    var generatedTestCases = ParseTestCases(
        aiResponse,
        request.ProjectId,
        moduleName);

    // Save into database
    _context.TestCases.AddRange(generatedTestCases);

    await _context.SaveChangesAsync();

    return _mapper.Map<IEnumerable<TestCaseResponseDto>>(
        generatedTestCases);
}

private async Task<List<string>> GetRelevantChunksAsync(int projectId, string prompt)
{
    try
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(prompt);

        return await _chromaDbService.SearchAsync(
            $"project-{projectId}",
            embedding,
            5);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Vector search failed for project {ProjectId}; falling back to stored document chunks.", projectId);
    }

    var dbChunks = await _context.DocumentChunks
        .Include(dc => dc.Document)
        .Where(dc => dc.Document != null && dc.Document.ProjectId == projectId)
        .OrderBy(dc => dc.DocumentId)
        .ThenBy(dc => dc.ChunkIndex)
        .Select(dc => dc.Content)
        .Where(content => !string.IsNullOrWhiteSpace(content))
        .ToListAsync();

    return dbChunks.Take(5).ToList();
}

private static string BuildPrompt(
    string userPrompt,
    List<string> documentChunks,
    string moduleName)
{
    var sb = new StringBuilder();

    sb.AppendLine("You are an expert Software QA Engineer.");
    sb.AppendLine();

    sb.AppendLine("Use ONLY the following software requirements to generate software test cases.");
    sb.AppendLine();

    sb.AppendLine("========== REQUIREMENT DOCUMENT ==========");

    foreach (var chunk in documentChunks)
    {
        sb.AppendLine(chunk);
        sb.AppendLine();
    }

    sb.AppendLine("==========================================");

    sb.AppendLine();

    sb.AppendLine($"User Request: {userPrompt}");
    sb.AppendLine($"Requested Module Name: {moduleName}");

    sb.AppendLine();

    sb.AppendLine("Generate software test cases in the following table format.");

    sb.AppendLine();

    sb.AppendLine("| Title | Type | Priority | Preconditions | Steps | Expected Result |");

    sb.AppendLine();

    sb.AppendLine("Rules:");

    sb.AppendLine("- Generate Positive test cases.");

    sb.AppendLine("- Generate Negative test cases.");

    sb.AppendLine("- Generate Edge test cases.");

    sb.AppendLine("- Generate Regression test cases.");

    sb.AppendLine("- Use High, Medium or Low priority.");

    sb.AppendLine("- Each test case must be unique.");

    sb.AppendLine("- Use clear software testing terminology.");

    sb.AppendLine("- Assign every generated test case to the exact module name supplied in the requested module name field.");

    sb.AppendLine("- Never use General as the module name when a specific module is requested.");

    sb.AppendLine("- Return ONLY the table.");

    return sb.ToString();
}


private static string NormalizeModuleName(string? moduleName, string? prompt)
{
    if (!string.IsNullOrWhiteSpace(moduleName))
        return moduleName.Trim();

    if (!string.IsNullOrWhiteSpace(prompt))
    {
        var promptWords = prompt
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToArray();

        if (promptWords.Length > 0)
            return promptWords[0];
    }

    return "General";
}

private List<TestCase> ParseTestCases(
    string aiResponse,
    int projectId,
    string moduleName)
{
    var testCases = new List<TestCase>();

    var lines = aiResponse
        .Split('\n', StringSplitOptions.RemoveEmptyEntries);

    foreach (var line in lines)
    {
        // Skip markdown header
        if (!line.StartsWith("|"))
            continue;

        if (line.Contains("Title"))
            continue;

        if (line.Contains("---"))
            continue;

        var columns = line
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();

        if (columns.Length < 6)
            continue;

        var testCase = new TestCase
        {
            ProjectId = projectId,
            ModuleName = moduleName,
            Title = columns[0],
            TestType = columns[1],
            Priority = columns[2],
            Preconditions = columns[3],
            TestSteps = columns[4],
            ExpectedResult = columns[5]
        };

        testCases.Add(testCase);
    }

    return testCases;
}


public async Task<TestCaseResponseDto?> UpdateAsync(
    int id,
    UpdateTestCaseDto dto)
{
    var testCase = await _context.TestCases
        .FirstOrDefaultAsync(x => x.Id == id);

    if (testCase == null)
        return null;

    testCase.ModuleName = dto.ModuleName;
    testCase.Title = dto.Title;
    testCase.TestType = dto.TestType;
    testCase.Priority = dto.Priority;
    testCase.Preconditions = dto.Preconditions;
    testCase.TestSteps = dto.TestSteps;
    testCase.ExpectedResult = dto.ExpectedResult;
    testCase.Description = dto.Description;

    await _context.SaveChangesAsync();

    return _mapper.Map<TestCaseResponseDto>(testCase);
}

public async Task<bool> DeleteAsync(int id)
{
    var testCase = await _context.TestCases
        .FirstOrDefaultAsync(x => x.Id == id);

    if (testCase == null)
        return false;

    _context.TestCases.Remove(testCase);

    await _context.SaveChangesAsync();

    return true;
}

    }
}