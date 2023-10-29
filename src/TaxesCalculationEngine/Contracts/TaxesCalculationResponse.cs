namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationResponse
{
    public Guid CorrelationId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public decimal TaxAAA { get; set; }
    public decimal TaxBBB { get; set; }
    public decimal TaxCCC { get; set; }
}