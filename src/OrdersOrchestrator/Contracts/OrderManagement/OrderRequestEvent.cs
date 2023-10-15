namespace OrdersOrchestrator.Contracts.OrderManagement;

public sealed class OrderRequestEvent
{
    // Could be an order id
    public Guid CorrelationId { get; set; }

    public string CustomerId { get; set; }

    public string ItemId { get; set; }
}