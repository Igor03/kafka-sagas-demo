namespace Orchestrator.Contracts;

public class ConsumerXRequest
{
    public ConsumerXRequest(string operation)
    {
        Operation = operation;
    }

    public string Operation { get; set; }
}