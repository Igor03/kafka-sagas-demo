using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class CustomerValidationStepActivity : IStateMachineActivity<OrderRequestState, CustomerValidationResponseEvent>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(
            BehaviorContext<OrderRequestState, CustomerValidationResponseEvent> context, 
            IBehavior<OrderRequestState, CustomerValidationResponseEvent> next)
        {
            await next.Execute(context);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestState, CustomerValidationResponseEvent, TException> context, 
            IBehavior<OrderRequestState, CustomerValidationResponseEvent> next) 
            where TException : Exception
        {
            await next.Execute(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("customer-validation");
        }
    }
}
