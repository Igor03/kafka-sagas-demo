using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class CustomerValidationStepActivity : IStateMachineActivity<OrderRequestState, CustomerValidationResponseEvent>
    {
        private readonly ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer;

        public CustomerValidationStepActivity(ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer)
        {
            this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestState, CustomerValidationResponseEvent> context, 
            IBehavior<OrderRequestState, CustomerValidationResponseEvent> next)
        {
            var taxesCalculationRequestEvent = new TaxesCalculationRequestEvent
            {
                CorrelationId = context.Saga.CorrelationId,
                ItemId = context.Saga.ItemId!,
                CustomerType = context.Message.CustomerType
            };

            await taxesCalculationEngineProducer.Produce(taxesCalculationRequestEvent);
            await next.Execute(context);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestState, CustomerValidationResponseEvent, TException> context, 
            IBehavior<OrderRequestState, CustomerValidationResponseEvent> next) 
            where TException : Exception => await next.Execute(context);

        public void Probe(ProbeContext context) => context.CreateScope("customer-validation");
        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    }
}
