using Contracts;
using MassTransit;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestSagaInstance : SagaStateMachineInstance, ISagaVersion
{
    public string? CurrentState { get; set; }
    public string? ItemId { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerType { get; set; }
    public NotificationReply<OrderResponseEvent>? NotificationReply { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Default props
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
}