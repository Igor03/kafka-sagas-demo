namespace Contracts;

public record TaxesCalculationRequestEvent
{
    public string CustomerType { get; set; } = default!;
    public string ItemId { get; set; } = default!;
}