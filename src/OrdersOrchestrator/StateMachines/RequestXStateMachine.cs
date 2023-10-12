using MassTransit;
using OrdersOrchestrator.Contracts;

namespace OrdersOrchestrator.StateMachines;

public sealed class RequestXStateMachine : MassTransitStateMachine<RequestXState>
{
    public RequestXStateMachine()
    {
        InstanceState(p => p.CurrentState);
        
    }
}