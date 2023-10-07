namespace Contracts;

public record TaxesCalculationResponseEvent
{
    public string ItemId { get; set; } = default!;
    public decimal TaxAAA { get; set; }
    public decimal TaxBBB { get; set; }
    public decimal TaxCCC { get; set; }
}