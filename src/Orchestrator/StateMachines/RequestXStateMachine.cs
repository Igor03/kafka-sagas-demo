using MassTransit;
using Orchestrator.Contracts;

namespace Orchestrator.StateMachines;

public sealed class RequestXStateMachine : MassTransitStateMachine<RequestXState>
{
    public RequestXStateMachine()
    {
        InstanceState(p => p.CurrentState);
        
    }
}