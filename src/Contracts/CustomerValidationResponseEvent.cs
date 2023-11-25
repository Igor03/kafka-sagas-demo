namespace Contracts;

public record CustomerValidationResponseEvent
{
    public string CustomerType { get; set; } = default!;
}

