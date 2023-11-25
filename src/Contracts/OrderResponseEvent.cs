namespace Contracts;

public record OrderResponseEvent
{
    public string CustomerId { get; set; } = default!;
    public string CustomerType { get; set; } = default!;
    public TaxesCalculationResponseEvent TaxesCalculation { get; set; } = default!;
}