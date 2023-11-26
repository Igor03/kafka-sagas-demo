namespace Contracts;

public record FaultMessageEvent
{
    public object? Event { get; set; } = default!;
    public string? ExceptionMessage { get; set; }
}