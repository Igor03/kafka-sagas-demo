namespace Producer.Controllers;

public class SourceSystemRequest
{
    public SourceSystemRequest(string operation)
    {
        Operation = operation;
    }

    public string Operation { get; set; }
}