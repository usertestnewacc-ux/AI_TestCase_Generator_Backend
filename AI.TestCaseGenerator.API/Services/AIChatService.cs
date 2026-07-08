using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.AIChat;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AI.TestCaseGenerator.API.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChromaDbService _chromaDbService;
        private readonly IClaudeService _claudeService;
        private readonly IMapper _mapper;

        public AIChatService(
            ApplicationDbContext context,
            IEmbeddingService embeddingService,
            IChromaDbService chromaDbService,
            IClaudeService claudeService,
            IMapper mapper)
        {
            _context = context;
            _embeddingService = embeddingService;
            _chromaDbService = chromaDbService;
            _claudeService = claudeService;
            _mapper = mapper;
        }

        public async Task<AIChatResponseDto> AskQuestionAsync(AIChatRequestDto request,int userId)
        {
    // Validate project ownership
    var project = await _context.Projects
        .FirstOrDefaultAsync(x =>
            x.Id == request.ProjectId &&
            x.UserId == userId);

    if (project == null)
        throw new Exception("Project not found.");

    string answer;

    try
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);

        var contextChunks = await _chromaDbService.SearchAsync(
            $"project-{project.Id}",
            embedding);

        var prompt = BuildPrompt(contextChunks, request.Question);

        answer = await _claudeService.GenerateResponseAsync(prompt);
    }
    catch
    {
        answer = "The AI assistant is currently unavailable. Please try again shortly.";
    }

    var history = new ChatHistory
    {
        ProjectId = project.Id,
        UserId = userId,
        UserQuestion = request.Question,
        AiResponse = answer
    };

    _context.ChatHistories.Add(history);

    await _context.SaveChangesAsync();

    return new AIChatResponseDto
    {
        Question = request.Question,
        Answer = answer,
        CreatedAt = history.CreatedAt
    };
    }

private string BuildPrompt(
    List<string> contextChunks,
    string question)
{
    var builder = new System.Text.StringBuilder();

    builder.AppendLine(
        "Answer only using the following project documentation.");

    builder.AppendLine();

    foreach (var chunk in contextChunks)
    {
        builder.AppendLine(chunk);
        builder.AppendLine();
    }

    builder.AppendLine("Question:");

    builder.AppendLine(question);

    builder.AppendLine();

    builder.AppendLine(
        "If the answer is not contained in the documents, clearly say you don't know.");

    return builder.ToString();
}

public async Task<IEnumerable<ChatHistoryDto>>
GetChatHistoryAsync(
    int projectId,
    int userId)
{
    var history = await _context.ChatHistories
        .Where(x =>
            x.ProjectId == projectId &&
            x.UserId == userId)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return _mapper.Map<IEnumerable<ChatHistoryDto>>(history);
}

public async Task<bool> DeleteChatHistoryAsync(
    int projectId,
    int userId)
{
    var chats = await _context.ChatHistories
        .Where(x =>
            x.ProjectId == projectId &&
            x.UserId == userId)
        .ToListAsync();

    if (!chats.Any())
        return false;

    _context.ChatHistories.RemoveRange(chats);

    await _context.SaveChangesAsync();

    return true;
}



    }
}