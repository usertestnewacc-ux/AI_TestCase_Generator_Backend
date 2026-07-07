using AI.TestCaseGenerator.API.DTOs.AIChat;

namespace AI.TestCaseGenerator.API.Interfaces
{
    public interface IAIChatService
    {
        Task<AIChatResponseDto> AskQuestionAsync(
            AIChatRequestDto request,
            int userId);

        Task<IEnumerable<ChatHistoryDto>> GetChatHistoryAsync(
            int projectId,
            int userId);

        Task<bool> DeleteChatHistoryAsync(
            int projectId,
            int userId);
    }
}