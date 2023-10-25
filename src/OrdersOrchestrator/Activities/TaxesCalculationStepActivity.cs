using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class TaxesCalculationStepActivity : IStateMachineActivity<OrderRequestState, TaxesCalculationResponseEvent>
    {
        private readonly ITopicProducer<OrderResponseEvent> orderResponseEventProducer;

        public TaxesCalculationStepActivity(ITopicProducer<OrderResponseEvent> orderResponseEventProducer)
        {
            this.orderResponseEventProducer = orderResponseEventProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestState, TaxesCalculationResponseEvent> context, 
            IBehavior<OrderRequestState, TaxesCalculationResponseEvent> next)
        {
            if (context.Message.ItemId.ToUpper() == "S_ERROR")
            {
                throw new Exception("Erro no segundo processo");
            }

            var orderResponseEvent = new OrderResponseEvent
            {
                CorrelationId = context.Saga.CorrelationId,
                CustomerId = context.Saga.CustomerId,
                CustomerType= context.Saga.CustomerType,
                TaxesCalculation = context.Message,
                FinishedAt = DateTime.Now,
            };

            await orderResponseEventProducer.Produce(orderResponseEvent);

            await next.Execute(context);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestState, TaxesCalculationResponseEvent, TException> context, 
            IBehavior<OrderRequestState, TaxesCalculationResponseEvent> next) 
            where TException : Exception
        {
            await next.Execute(context);
        }

        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
        public void Probe(ProbeContext context) => context.CreateScope("taxes-calculation");
    }
}
