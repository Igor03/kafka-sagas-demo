using MassTransit;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Services;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class ReceiveOrderRequestStepActivity : IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>
    {
        private readonly IApiService apiService;
        private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer;
        private readonly ITopicProducer<ErrorMessageEvent> deadLetterProducer;

        public ReceiveOrderRequestStepActivity(
            IApiService apiService, 
            ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer, 
            ITopicProducer<ErrorMessageEvent> deadLetterProducer)
        {
            this.apiService = apiService;
            this.customerValidationEngineProducer = customerValidationEngineProducer;
            this.deadLetterProducer = deadLetterProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestSagaInstance, OrderRequestEvent> context, 
            IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next)
        {
            Thread.Sleep(1500);

            if (await apiService
                .ValidateIncomingRequestAsync(context.Message)
                .ConfigureAwait(false))
            {
                throw new Exception(context.Saga.Reason!);
            }

            var customerValidationEvent = new
            {
                context.Message.CorrelationId,
                context.Message.CustomerId,
            };

            await customerValidationEngineProducer
                .Produce(customerValidationEvent);

            await next.Execute(context)
                .ConfigureAwait(false);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestSagaInstance, OrderRequestEvent, TException> context, 
            IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next) 
            where TException : Exception
        {
            var deadLetterEvent = new
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message,
                __Header_RetryAttempt = 2,
                __Header_Reason = context.Saga.Reason,
            };
                
            await deadLetterProducer.Produce(deadLetterEvent);

            await next.Execute(context)
                .ConfigureAwait(false);
        }
        

        public void Probe(ProbeContext context) => context.CreateScope("order-placed");
        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
        
    }
}
