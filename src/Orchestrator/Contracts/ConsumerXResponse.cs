namespace Orchestrator.Contracts;

public class ConsumerXResponse
{
    public ConsumerXResponse(bool response, DateTime repliedAt)
    {
        Response = response;
        RepliedAt = repliedAt;
    }

    public bool Response { get; set; }
    
    public DateTime RepliedAt { get; set; }
}