using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Contracts.OrderManagement;

public sealed class OrderResponse
{
    public OrderResponse(Guid orderId, TaxesCalculationResponse taxesCalculation, DateTime finishedAt)
    {
        OrderId = orderId;
        TaxesCalculation = taxesCalculation;
        FinishedAt = finishedAt;
    }

    public OrderResponse()
    {
    }
    
    public Guid OrderId { get; set; }
    
    public TaxesCalculationResponse TaxesCalculation { get; set; }
    
    public DateTime FinishedAt { get; set; }
}