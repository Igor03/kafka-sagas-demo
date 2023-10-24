using MassTransit;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class TaxesCalculationStepActivity : IStateMachineActivity<OrderRequestState, OrderRequestEvent>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(
            BehaviorContext<OrderRequestState, OrderRequestEvent> context, 
            IBehavior<OrderRequestState, OrderRequestEvent> next)
        {
            await next.Execute(context);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestState, OrderRequestEvent, TException> context, 
            IBehavior<OrderRequestState, OrderRequestEvent> next) 
            where TException : Exception
        {
            await next.Execute(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("taxes-calculation");
        }
    }
}
