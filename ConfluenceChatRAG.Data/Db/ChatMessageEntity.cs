using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConfluenceChatRAG.Data.Models.Dto;

namespace ConfluenceChatRAG.Data.Db;

public class ChatMessageEntity
{
    public int Id { get; set; }

    public string SessionId { get; set; }
    public bool IsUser { get; set; } // true for user message, false for assistant
    public string Content { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public List<ChatSourceDto> Sources { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
}