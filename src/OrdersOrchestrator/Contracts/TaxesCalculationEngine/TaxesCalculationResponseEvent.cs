namespace OrdersOrchestrator.Contracts.TaxesCalculationEngine;

public sealed class TaxesCalculationResponseEvent
{
    public Guid CorrelationId { get; set; }
    public string? ItemId { get; set; }
    
    public decimal TaxAAA { get; set; }
    
    public decimal TaxBBB { get; set; }
    
    public decimal TaxCCC { get; set; }
}