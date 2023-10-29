namespace OrdersManagementApi.Contracts;

public sealed class OrderRequest
{
    public OrderRequest(Guid correlationId, string customerId, string itemId)
    {
        CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId;
        CustomerId = customerId;
        ItemId = itemId;
    }

    public Guid CorrelationId { get; set; }
    public string CustomerId { get; set; }
    public string ItemId { get; set; }
}