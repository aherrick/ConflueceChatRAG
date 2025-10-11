using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using ConfluenceChatRAG.Data.Db;
using ConfluenceChatRAG.Data.Models.Config;
using ConfluenceChatRAG.Data.Models.Dto;
using OpenAI.Chat;

namespace ConfluenceChatRAG.Data.Services;

public class ChatCompletionService
{
    private readonly ChatClient _chatClient;
    private readonly SearchService _searchService;

    public ChatCompletionService(AppConfig config)
    {
        var openAIClient = new AzureOpenAIClient(
            new Uri(config.AzureOpenAI.Endpoint),
            new AzureKeyCredential(config.AzureOpenAI.ApiKey)
        );

        _chatClient = openAIClient.GetChatClient(config.AzureOpenAI.ChatDeployment);
        _searchService = new SearchService(config);
    }

    /// <summary>
    /// RAG with tool-calling: model decides when to search.
    /// Simple loop + switch; assumes 'query' always present in tool args.
    /// Uses the requested foreach/if-else pattern for tool handling.
    /// </summary>
    public async Task<(
        string answer,
        List<ChatSourceDto> sources,
        List<string> suggestions
    )> GetAnswerWithToolsAsync(string question, List<ChatMessageEntity> chatHistory = null)
    {
        var conversation = new List<ChatMessage>
        {
            new SystemChatMessage(
                """
You are the Confluence Assistant for our product documentation.

Only answer questions directly related to our product, its features, pricing (if documented), architecture, repositories, Azure setup, or Confluence pages.

If a question isn't covered in our documentation or product materials, reply exactly with:
"That's a bit outside what I can help with — I'm here to answer questions about our docs and product. Could you rephrase it in that scope?"

Use the search_docs tool when needed to find answers in our documentation.
Be concise and factual.

You may format your **answer** using basic Markdown (headings, bold/italics, bullet lists, and fenced code blocks). Do **not** use Markdown in the SUGGESTIONS line.

IMPORTANT: At the end of your response, provide 2-3 relevant follow-up questions that the user might want to ask next.
You MUST end your response with this EXACT format (all on one line after your answer):

###SUGGESTIONS### ["First question?", "Second question?", "Third question?"]

Do NOT add line breaks in the suggestions section. Keep the ### delimiter and JSON array on the same line.
"""
            ),
        };

        if (chatHistory is { Count: > 0 })
        {
            conversation.AddRange(
                chatHistory.Select(m =>
                    m.IsUser
                        ? (ChatMessage)new UserChatMessage(m.Content)
                        : new AssistantChatMessage(m.Content)
                )
            );
        }

        conversation.Add(new UserChatMessage(question));

        var sources = new List<ChatSourceDto>();

        var searchTool = ChatTool.CreateFunctionTool(
            functionName: "search_docs",
            functionDescription: "Searches Confluence documentation for relevant information.",
            functionParameters: BinaryData.FromString(
                """
                {
                  "type": "object",
                  "properties": {
                    "query": { "type": "string" }
                  },
                  "required": ["query"]
                }
                """
            )
        );

        var opts = new ChatCompletionOptions { Tools = { searchTool } };

        var requestCompletion = true;
        var finalText = "";

        while (requestCompletion)
        {
            var completion = (await _chatClient.CompleteChatAsync(conversation, opts)).Value;
            conversation.Add(new AssistantChatMessage(completion));

            switch (completion.FinishReason)
            {
                case ChatFinishReason.ToolCalls:
                    foreach (var call in completion.ToolCalls)
                    {
                        if (call.FunctionName == "search_docs")
                        {
                            var args = JsonSerializer.Deserialize<Dictionary<string, string>>(
                                call.FunctionArguments
                            );
                            var query = args["query"];

                            var results = await _searchService.SearchAsync(query, top: 5);
                            if (results?.Count > 0)
                                sources.AddRange(results);

                            var toolJson = JsonSerializer.Serialize(
                                results?.Select(r => new
                                {
                                    r.Title,
                                    r.Content,
                                    r.Url,
                                }) ?? Enumerable.Empty<object>()
                            );

                            conversation.Add(
                                new ToolChatMessage(
                                    call.Id,
                                    [ChatMessageContentPart.CreateTextPart(toolJson)]
                                )
                            );
                        }
                    }
                    // next loop iteration calls CompleteChatAsync again with tool outputs included
                    break;

                case ChatFinishReason.Stop:
                    finalText = string.Concat(completion.Content.Select(p => p.Text)).Trim();
                    requestCompletion = false;
                    break;
            }
        }

        // Extract suggestions from the response
        var (answer, suggestions) = ExtractSuggestions(finalText);

        return (answer, sources, suggestions);
    }

    /// <summary>
    /// Extract suggestions from the AI response
    /// </summary>
    private static (string answer, List<string> suggestions) ExtractSuggestions(string response)
    {
        var suggestions = new List<string>();

        // Look for the suggestions delimiter - try multiple patterns for robustness
        var delimiterIndex = response.IndexOf(
            "###SUGGESTIONS###",
            StringComparison.OrdinalIgnoreCase
        );

        if (delimiterIndex == -1)
        {
            // No suggestions found, return the whole response
            return (response.Trim(), suggestions);
        }

        var answer = response[..delimiterIndex].Trim();
        var suggestionsText = response[delimiterIndex..].Trim();

        // Find the JSON array in the suggestions text
        var jsonStartIndex = suggestionsText.IndexOf('[');
        var jsonEndIndex = suggestionsText.LastIndexOf(']');

        if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
        {
            var suggestionsJson = suggestionsText.Substring(
                jsonStartIndex,
                jsonEndIndex - jsonStartIndex + 1
            );

            try
            {
                // Deserialize the JSON array of suggestions
                suggestions = JsonSerializer.Deserialize<List<string>>(suggestionsJson) ?? [];
            }
            catch (JsonException)
            {
                // If JSON parsing fails, just return empty suggestions
            }
        }

        return (answer, suggestions);
    }
}