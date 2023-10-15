using MassTransit;

namespace OrdersOrchestrator.StateMachines;

public sealed class OrderRequestState : SagaStateMachineInstance, ISagaVersion
{
    public int CurrentState { get; set; }
    
    public string ItemId { get; set; }
    
    public string CustomerId { get; set; }
    
    public Guid CorrelationId { get; set; }
    
    public int Version { get; set; }
}