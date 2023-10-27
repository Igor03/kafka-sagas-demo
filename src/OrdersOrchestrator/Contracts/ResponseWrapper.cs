namespace OrdersOrchestrator.Contracts
{
    public class ResponseWrapper
    {
        public bool Success { get; set; }
        public string? Reason { get; set; }
        public object? Data { get; set; }
    }
}
