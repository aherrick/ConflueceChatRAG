using ConfluenceChatRAG.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConfluenceChatRAG.Fn;

/// <summary>
/// Optional endpoints for managing chat sessions
/// </summary>
public class ChatSession(ILogger<ChatSession> logger, ChatHistoryService historyService)
{
    /// <summary>
    /// Get chat history for a session
    /// GET /api/ChatSession/{sessionId}/history
    /// </summary>
    [Function("GetSessionHistory")]
    public async Task<IActionResult> GetSessionHistory(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "ChatSession/{sessionId}/history"
        )]
            HttpRequest req,
        string sessionId
    )
    {
        var entities = await historyService.GetHistoryAsync(sessionId);

        if (entities.Count == 0)
        {
            return new NotFoundObjectResult(new { error = "Session not found" });
        }

        // Only include suggestions for the last assistant message
    var lastAssistantEntry = entities.LastOrDefault(e => !e.IsUser);
        foreach (var entry in entities)
        {
            if (entry != lastAssistantEntry)
            {
                entry.Suggestions = [];
            }
        }

        logger.LogInformation(
            "Retrieved history for session: {SessionId}, {Count} messages",
            sessionId,
            entities.Count
        );

        return new OkObjectResult(entities);
    }
}