namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationRequest
{
    public Guid CorrelationId { get; set; }
    public string? CustomerType { get; set; }
    public string? ItemId { get; set; }
}