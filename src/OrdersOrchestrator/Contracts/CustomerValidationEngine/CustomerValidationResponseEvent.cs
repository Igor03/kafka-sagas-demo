namespace OrdersOrchestrator.Contracts.CustomerValidationEngine
{
    public record CustomerValidationResponseEvent
    {
        public Guid CorrelationId { get; set; }
        public string CustomerType { get; set; } = string.Empty;
    }
}
