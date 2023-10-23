namespace OrdersOrchestrator.Contracts.CustomerValidationEngine
{
    public sealed class CustomerValidationResponseEvent
    {
        public Guid CorrelationId { get; set; }
        public string CustomerType { get; set; } = string.Empty;
    }
}
