namespace OrdersOrchestrator.Contracts.OrderManagement;

public sealed class OrderRequest
{
    public OrderRequest(Guid orderId, string customerId, string itemId)
    {
        OrderId = orderId;
        CustomerId = customerId;
        ItemId = itemId;
    }

    public OrderRequest()
    {
    }
    
    public Guid OrderId { get; set; }

    public string CustomerId { get; set; }

    public string ItemId { get; set; }
}