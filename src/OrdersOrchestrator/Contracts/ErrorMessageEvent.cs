namespace OrdersOrchestrator.Contracts
{
    public class ErrorMessageEvent    
    {
        public Guid CorrelationId { get; set; }

        public object? Message { get; set; }
    }
}
