namespace Orchestrator.Contracts;

public class SourceSystemResponse
{
    public SourceSystemResponse(bool response, DateTime repliedAt)
    {
        Response = response;
        RepliedAt = repliedAt;
    }

    public bool Response { get; set; }
    
    public DateTime RepliedAt { get; set; }
}