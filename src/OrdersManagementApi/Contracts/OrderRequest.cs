namespace OrdersManagementApi.Contracts;

public sealed class OrderRequest
{
    public OrderRequest(Guid orderId, string customerId, string itemId)
    {
        OrderId = orderId == Guid.Empty ? Guid.NewGuid() : orderId;
        CustomerId = customerId;
        ItemId = itemId;
    }

    public Guid OrderId { get; set; }

    public string CustomerId { get; set; }

    public string ItemId { get; set; }
}