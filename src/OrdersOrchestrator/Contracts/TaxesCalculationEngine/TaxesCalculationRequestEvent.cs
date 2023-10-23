namespace OrdersOrchestrator.Contracts.TaxesCalculationEngine;

public sealed class TaxesCalculationRequestEvent
{
    public Guid CorrelationId { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
}