using ConfluenceChatRAG.Data.Models.Config;
using ConfluenceChatRAG.Data.Models.Dto;
using ConfluenceChatRAG.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfluenceChatRAG.Fn;

public class Chat(ILogger<Chat> logger, IConfiguration config, ChatHistoryService historyService)
{
    [Function(nameof(Chat))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
    )
    {
        var appConfig = config.Get<AppConfig>();

        // Read and deserialize request body
        ChatRequestDto request = await req.ReadFromJsonAsync<ChatRequestDto>();

        // Handle session creation/retrieval
        var sessionId =
            !string.IsNullOrEmpty(request.SessionId)
            && await historyService.SessionExistsAsync(request.SessionId)
                ? request.SessionId
                : historyService.CreateSession();

        logger.LogInformation(
            "{Action} chat session: {SessionId}",
            request.SessionId == sessionId ? "Continuing" : "Created new",
            sessionId
        );

        logger.LogInformation("Chat question: {Question}", request.Question);

        // Get chat history for this session
        var history = await historyService.GetHistoryAsync(sessionId);
        logger.LogInformation("Session has {Count} previous messages", history.Count);

        // Chat completion with function calling - LLM decides when to search
        var chatService = new ChatCompletionService(appConfig);
        var (answer, sources, suggestions) = await chatService.GetAnswerWithToolsAsync(
            request.Question,
            history
        );

        logger.LogInformation(
            "Response generated. Sources used: {Count}, Suggestions: {SuggestionCount}",
            sources.Count,
            suggestions.Count
        );

        // Store the current exchange in history (with metadata for assistant message)
        await historyService.AddUserMessageAsync(sessionId, request.Question);
        await historyService.AddAssistantMessageAsync(sessionId, answer, sources, suggestions);

        return new OkObjectResult(
            new ChatResponseDto
            {
                SessionId = sessionId,
                Question = request.Question,
                Answer = answer,
                Sources = sources,
                Suggestions = suggestions,
            }
        );
    }
}