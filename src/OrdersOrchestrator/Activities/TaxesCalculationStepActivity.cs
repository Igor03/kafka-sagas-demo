using MassTransit;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class TaxesCalculationStepActivity : IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>
    {
        private readonly ITopicProducer<OrderResponseEvent> orderResponseEventProducer;

        public TaxesCalculationStepActivity(ITopicProducer<OrderResponseEvent> orderResponseEventProducer)
        {
            this.orderResponseEventProducer = orderResponseEventProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent> context, 
            IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next)
        {
            Thread.Sleep(1500);

            if (context.Message.ItemId!.ToUpper() == "S_ERROR")
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
            BehaviorExceptionContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent, TException> context, 
            IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next) 
            where TException : Exception
        {
            await next.Execute(context);
        }

        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
        public void Probe(ProbeContext context) => context.CreateScope("taxes-calculation");
    }
}
