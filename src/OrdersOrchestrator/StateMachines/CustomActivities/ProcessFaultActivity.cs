using Contracts;
using MassTransit;

namespace OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class ProcessFaultActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, FaultMessageEvent>
{
    Task IStateMachineActivity<OrderRequestSagaInstance, FaultMessageEvent>.Execute(
        BehaviorContext<OrderRequestSagaInstance, FaultMessageEvent> context, 
        IBehavior<OrderRequestSagaInstance, FaultMessageEvent> next)
    {
        throw new NotImplementedException();
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, FaultMessageEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, FaultMessageEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, FaultMessageEvent> next) 
            => await next.Faulted(context).ConfigureAwait(false);
    
    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(ProcessFaultActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}