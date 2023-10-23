namespace OrdersOrchestrator.Contracts.CustomerValidationEngine
{
    public sealed class CustomerValidationRequestEvent
    {
        public Guid CorrelationId { get; set; }
        public string? CustomerId { get; set; }
    }
}
