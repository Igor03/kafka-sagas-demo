namespace OrdersOrchestrator.Contracts
{
    public record RetryDelayExpired(Guid CorrelationId);
}
