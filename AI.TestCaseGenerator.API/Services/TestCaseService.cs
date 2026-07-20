using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.TestCase;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public TestCaseService(
            ApplicationDbContext context,
            IMapper mapper,
            IOllamaChatService ollamaChatService,
            IOllamaEmbeddingService embeddingService,
            IChromaDbService chromaDbService)
        {
            _context = context;
            _mapper = mapper;
            _ollamaChatService = ollamaChatService;
            _embeddingService = embeddingService;
            _chromaDbService = chromaDbService;
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

    // Generate embedding for user prompt
    var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Prompt);

    // Search relevant chunks from ChromaDB using the same project-specific collection as indexing
    var relevantChunks = await _chromaDbService.SearchAsync(
        $"project-{project.Id}",
        embedding,
        5);

    // Build RAG prompt
    var prompt = BuildPrompt(request.Prompt, relevantChunks);

    // Send to Ollama
    var aiResponse = await _ollamaChatService.AskAsync(prompt);

    // Parse AI response
    var generatedTestCases = ParseTestCases(
        aiResponse,
        request.ProjectId);

    // Save into database
    _context.TestCases.AddRange(generatedTestCases);

    await _context.SaveChangesAsync();

    return _mapper.Map<IEnumerable<TestCaseResponseDto>>(
        generatedTestCases);
}

private static string BuildPrompt(
    string userPrompt,
    List<string> documentChunks)
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

    sb.AppendLine("- Return ONLY the table.");

    return sb.ToString();
}


private List<TestCase> ParseTestCases(
    string aiResponse,
    int projectId)
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

    testCase.Title = dto.Title;
    testCase.TestType = dto.TestType;
    testCase.Priority = dto.Priority;
    testCase.Preconditions = dto.Preconditions;
    testCase.TestSteps = dto.TestSteps;
    testCase.ExpectedResult = dto.ExpectedResult;

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