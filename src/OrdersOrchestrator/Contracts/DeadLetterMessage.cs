namespace OrdersOrchestrator.Contracts
{
    public class DeadLetterMessage    
    {
        public Guid CorrelationId { get; set; }

        public object? Message { get; set; }
    }
}
