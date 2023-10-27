using MassTransit;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities;

public sealed class ProcessFaultedMessageStepActivity : IStateMachineActivity<OrderRequestSagaInstance, ErrorMessageEvent>
{

    public async Task Execute(
        BehaviorContext<OrderRequestSagaInstance, ErrorMessageEvent> context, 
        IBehavior<OrderRequestSagaInstance, ErrorMessageEvent> next)
    {
        _ = context.Saga;
        
        await next.Execute(context)
            .ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, ErrorMessageEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, ErrorMessageEvent> next) 
        where TException : Exception
    {
        throw new NotImplementedException();
    }
    
    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    public void Probe(ProbeContext context) => context.CreateScope("taxes-calculation");
}