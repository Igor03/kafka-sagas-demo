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
        private readonly ITopicProducer<ErrorMessageEvent> errorProducer;

        public ReceiveOrderRequestStepActivity(
            IApiService apiService, 
            ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer, 
            ITopicProducer<ErrorMessageEvent> errorProducer)
        {
            this.apiService = apiService;
            this.customerValidationEngineProducer = customerValidationEngineProducer;
            this.errorProducer = errorProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestSagaInstance, OrderRequestEvent> context, 
            IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next)
        {
            if (await apiService
                .ValidateIncomingRequestAsync(context.Message)
                .ConfigureAwait(false))
            {
                throw new Exception("Some validation error was thrown");
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

            context.Saga.Reason = context.Exception.Message;
            context.Saga.UpdatedAt = DateTime.Now;
            
            // In case we need to process more complex fault compensation routines
            var errorEvent = new
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message,
                __Header_Reason = context.Exception.Message,
            };
                
            await errorProducer
                .Produce(errorEvent);

            await next.Execute(context)
                .ConfigureAwait(false);
        }
        
        public void Probe(ProbeContext context) => context.CreateScope("order-placed");
        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
        
    }
}
