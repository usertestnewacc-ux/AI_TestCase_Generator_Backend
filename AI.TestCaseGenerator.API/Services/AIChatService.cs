using AI.TestCaseGenerator.API.Data;
using AI.TestCaseGenerator.API.DTOs.AIChat;
using AI.TestCaseGenerator.API.Entities;
using AI.TestCaseGenerator.API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AI.TestCaseGenerator.API.Services
{
    public class AIChatService : IAIChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOllamaEmbeddingService _embeddingService;
        private readonly IChromaDbService _chromaDbService;
        private readonly IOllamaChatService _ollamaChatService;
        private readonly IMapper _mapper;

        public AIChatService(
            ApplicationDbContext context,
            IOllamaEmbeddingService embeddingService,
            IChromaDbService chromaDbService,
            IOllamaChatService ollamaChatService,
            IMapper mapper)
        {
            _context = context;
            _embeddingService = embeddingService;
            _chromaDbService = chromaDbService;
            _ollamaChatService = ollamaChatService;
            _mapper = mapper;
        }

        public async Task<AIChatResponseDto> AskQuestionAsync(AIChatRequestDto request,int userId)
        {
            try
            {
                // Validate project ownership
                var project = await _context.Projects
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ProjectId &&
                        x.UserId == userId);

                if (project == null)
                    return new AIChatResponseDto
                    {
                        Success = false,
                        Question = request.Question,
                        Answer = "Project not found.",
                        CreatedAt = DateTime.UtcNow
                    };

                var sw = Stopwatch.StartNew();

                // Generate embedding for user's question. If embeddings fail (rate limits, auth),
                // degrade gracefully to an empty context so Claude can still attempt a direct answer.
                float[]? embedding = null;
                try
                {
                    embedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);
                }
                catch (Exception embEx)
                {
                    // Log and continue with no embedding context
                    System.Diagnostics.Debug.WriteLine("Embedding generation failed: " + embEx.Message);
                }

                // Search ChromaDB only when we have an embedding
                List<string>? contextChunks = null;
                try
                {
                    if (embedding != null && embedding.Length > 0)
                    {
                        contextChunks = (await _chromaDbService.SearchAsync($"project-{project.Id}", embedding)).ToList();
                    }
                    else
                    {
                        contextChunks = new List<string>();
                    }
                }
                catch (Exception chromaEx)
                {
                    System.Diagnostics.Debug.WriteLine("ChromaDB search failed: " + chromaEx.Message);
                    contextChunks = new List<string>();
                }

                // Build prompt
                var prompt = BuildPrompt(contextChunks, request.Question);

                string answer;
                try
                {
                    System.Diagnostics.Debug.WriteLine("Sending prompt to Ollama");
                    answer = await _ollamaChatService.AskAsync(prompt);
                }
                catch (Exception)
                {
                    // Provide a best-effort reply when external AI services are unavailable.
                    if (contextChunks != null && contextChunks.Any())
                    {
                        var summary = string.Join("\n\n", contextChunks.Take(3));
                        answer = "[AI service unavailable] Retrieved context:\n\n" + summary;

                        var historyFallback = new ChatHistory
                        {
                            ProjectId = project.Id,
                            UserId = userId,
                            UserQuestion = request.Question,
                            AiResponse = answer
                        };

                        _context.ChatHistories.Add(historyFallback);
                        await _context.SaveChangesAsync();

                        sw.Stop();

                        return new AIChatResponseDto
                        {
                            Success = true,
                            Question = request.Question,
                            Answer = answer,
                            RetrievedChunks = contextChunks.Count,
                            ResponseTimeMs = sw.ElapsedMilliseconds,
                            CreatedAt = historyFallback.CreatedAt
                        };
                    }
                    else
                    {
                        answer = "[AI service unavailable] I can't reach the local Ollama service right now. Please ensure Ollama is running and the selected model is available. You can also upload project documents so I can answer from those when the AI service is unavailable.";

                        var historyFallback = new ChatHistory
                        {
                            ProjectId = project.Id,
                            UserId = userId,
                            UserQuestion = request.Question,
                            AiResponse = answer
                        };

                        _context.ChatHistories.Add(historyFallback);
                        await _context.SaveChangesAsync();

                        sw.Stop();

                        return new AIChatResponseDto
                        {
                            Success = true,
                            Question = request.Question,
                            Answer = answer,
                            RetrievedChunks = 0,
                            ResponseTimeMs = sw.ElapsedMilliseconds,
                            CreatedAt = historyFallback.CreatedAt
                        };
                    }
                }

                // Save chat history
                var history = new ChatHistory
                {
                    ProjectId = project.Id,
                    UserId = userId,
                    UserQuestion = request.Question,
                    AiResponse = answer
                };

                _context.ChatHistories.Add(history);
                await _context.SaveChangesAsync();

                sw.Stop();

                return new AIChatResponseDto
                {
                    Success = true,
                    Question = request.Question,
                    Answer = answer,
                    RetrievedChunks = contextChunks?.Count ?? 0,
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    CreatedAt = history.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new AIChatResponseDto
                {
                    Success = false,
                    Question = request?.Question ?? string.Empty,
                    Answer = "AI chat failed: " + ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
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