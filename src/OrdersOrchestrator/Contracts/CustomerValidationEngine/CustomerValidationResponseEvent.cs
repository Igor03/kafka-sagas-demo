namespace OrdersOrchestrator.Contracts.CustomerValidationEngine
{
    public sealed class CustomerValidationResponseEvent
    {
        public Guid CorrelationId { get; set; }

        public string AdjudtedCustomerId { get; set; }
    }
}
