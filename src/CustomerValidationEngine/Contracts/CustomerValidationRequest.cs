namespace CustomerValidationEngine.Contracts;

public sealed class CustomerValidationRequest
{
    public Guid CorrelationId { get; set; }
    public string? CustomerId { get; set; }
}