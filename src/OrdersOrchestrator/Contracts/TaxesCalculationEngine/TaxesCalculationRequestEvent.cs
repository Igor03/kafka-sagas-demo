namespace OrdersOrchestrator.Contracts.TaxesCalculationEngine;

public sealed class TaxesCalculationRequestEvent
{
    public Guid CorrelationId { get; set; }
    
    public string ItemId { get; set; }
}