using ConfluenceChatRAG.Data.Db;
using ConfluenceChatRAG.Data.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace ConfluenceChatRAG.Data.Services;

public class ChatHistoryService(IDbContextFactory<ChatHistoryDbContext> contextFactory)
{
    private const int MaxMessagesPerSession = 20;

    public string CreateSession()
    {
        return Guid.NewGuid().ToString();
    }

    public async Task<List<ChatMessageEntity>> GetHistoryAsync(string sessionId)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        // DbContext is configured for NoTracking globally. If we later need to update
        // an entity fetched here, re-query with AsTracking() or call context.ChatMessages.Update(entity).
        return await context
            .ChatMessages.Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task AddUserMessageAsync(string sessionId, string message)
    {
        using var context = await contextFactory.CreateDbContextAsync();

        context.ChatMessages.Add(
            new ChatMessageEntity
            {
                SessionId = sessionId,
                Role = "user",
                Content = message,
                Timestamp = DateTimeOffset.UtcNow,
            }
        );

        await context.SaveChangesAsync();
        await TrimHistoryAsync(context, sessionId);
    }

    public async Task AddAssistantMessageAsync(
        string sessionId,
        string message,
        List<ChatSourceDto> sources = null,
        List<string> suggestions = null
    )
    {
        using var context = await contextFactory.CreateDbContextAsync();

        var entity = new ChatMessageEntity
        {
            SessionId = sessionId,
            Role = "assistant",
            Content = message,
            Timestamp = DateTimeOffset.UtcNow,
        };

        if (sources?.Count > 0)
            entity.Sources = sources;
        if (suggestions?.Count > 0)
            entity.Suggestions = suggestions;

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync();
        await TrimHistoryAsync(context, sessionId);
    }

    public async Task<bool> SessionExistsAsync(string sessionId)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.ChatMessages.AnyAsync(m => m.SessionId == sessionId);
    }

    private static async Task TrimHistoryAsync(ChatHistoryDbContext context, string sessionId)
    {
        var count = await context.ChatMessages.CountAsync(m => m.SessionId == sessionId);

        if (count > MaxMessagesPerSession)
        {
            var toRemove = await context
                .ChatMessages.Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .Take(count - MaxMessagesPerSession)
                .ToListAsync();

            context.ChatMessages.RemoveRange(toRemove);
            await context.SaveChangesAsync();
        }
    }
}