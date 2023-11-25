namespace Contracts;

public record FaultMessageEvent
{
    public object Message { get; set; } = default!;
    public Exception? Exception { get; set; }
}