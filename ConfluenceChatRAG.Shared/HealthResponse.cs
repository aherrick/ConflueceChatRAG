namespace ConfluenceChatRAG.Shared;

public class HealthResponse
{
    public string Status { get; set; }
    public string Environment { get; set; }
    public string ChatDeployment { get; set; }
    public DateTime Timestamp { get; set; }
}