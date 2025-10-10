using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConfluenceChatRAG.Data.Models.Dto;

namespace ConfluenceChatRAG.Data.Db;

public class ChatMessageEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SessionId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } // "user" or "assistant"

    [Required]
    public string Content { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    // Stored in database as JSON
    [JsonIgnore]
    public string SourcesJson { get; set; }

    [JsonIgnore]
    public string SuggestionsJson { get; set; }

    // Not stored in database - computed from JSON columns
    [NotMapped]
    public List<ChatSourceDto> Sources
    {
        get =>
            string.IsNullOrEmpty(SourcesJson)
                ? []
                : JsonSerializer.Deserialize<List<ChatSourceDto>>(SourcesJson) ?? [];
        set => SourcesJson = value?.Count > 0 ? JsonSerializer.Serialize(value) : null;
    }

    [NotMapped]
    public List<string> Suggestions
    {
        get =>
            string.IsNullOrEmpty(SuggestionsJson)
                ? []
                : JsonSerializer.Deserialize<List<string>>(SuggestionsJson) ?? [];
        set => SuggestionsJson = value?.Count > 0 ? JsonSerializer.Serialize(value) : null;
    }
}