namespace Contracts;

public record CustomerValidationRequestEvent
{
    public string CustomerId { get; set; } = string.Empty;
}

