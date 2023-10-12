using MassTransit;

namespace OrdersOrchestrator.StateMachines;

public sealed class RequestXState : SagaStateMachineInstance, ISagaVersion
{
    public string CurrentState { get; set; } = string.Empty;
    public string? Operation { get; set; } = string.Empty;
    
    // Required props
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
}