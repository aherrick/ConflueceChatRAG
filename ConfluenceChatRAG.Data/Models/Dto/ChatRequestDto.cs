namespace ConfluenceChatRAG.Data.Models.Dto;

public class ChatRequestDto
{
    public string Question { get; set; }
    public string SessionId { get; set; } // If null or empty, creates a new session
}