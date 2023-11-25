namespace Contracts;

public record ErrorMessageEvent
{
    public object Message { get; set; } = default!;
    public Exception? Exception { get; set; }
}

