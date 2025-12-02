using ConfluenceChatRAG.Data.Models.Config;
using Microsoft.Extensions.AI;

namespace ConfluenceChatRAG.Data.Services;

public class SearchService(AppConfig config)
{
    private readonly EmbeddingService _embeddingService = new(config);

    public async Task<List<ChatSourceDto>> SearchAsync(string query, int top = 3)
    {
        var queryEmbedding = await _embeddingService.Generator.GenerateVectorAsync(
            query ?? string.Empty
        );
        var results = new List<ChatSourceDto>();

        await foreach (
            var hit in _embeddingService.Collection.SearchAsync(queryEmbedding, top: top)
        )
        {
            results.Add(
                new ChatSourceDto
                {
                    Title = hit.Record.Name,
                    Content = hit.Record.Description,
                    Score = hit.Score.GetValueOrDefault(),
                    Url = hit.Record.Url,
                }
            );
        }

        return results;
    }
}