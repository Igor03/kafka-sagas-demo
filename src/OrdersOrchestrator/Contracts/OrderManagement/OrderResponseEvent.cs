using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Contracts.OrderManagement;

public sealed class OrderResponseEvent
{
    // Could be an order id
    public Guid CorrelationId { get; set; }
    
    public TaxesCalculationResponseEvent TaxesCalculation { get; set; }
    
    public DateTime FinishedAt { get; set; }
}