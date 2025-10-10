namespace ConfluenceChatRAG.Data.Models.Dto;

public class ChatResponseDto
{
    public string SessionId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public List<ChatSourceDto> Sources { get; set; }
    public List<string> Suggestions { get; set; } = [];
}