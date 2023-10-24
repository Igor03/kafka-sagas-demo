using Confluent.Kafka;
using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Services;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class ReceiveOrderRequestStepActivity : IStateMachineActivity<OrderRequestState, OrderRequestEvent>
    {
        private readonly IApiService apiService;
        private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer;

        public ReceiveOrderRequestStepActivity(
            IApiService apiService, 
            ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer)
        {
            this.apiService = apiService;
            this.customerValidationEngineProducer = customerValidationEngineProducer;
        }

        public async Task Execute(
            BehaviorContext<OrderRequestState, OrderRequestEvent> context, 
            IBehavior<OrderRequestState, OrderRequestEvent> next)
        {
            if (await apiService
                .ValidateIncomingRequestAsync(context.Message)
                .ConfigureAwait(false))
                    throw new Exception("Something wrong just happened here");

            var customerValidationEvent = new
            {
                context.Message.CorrelationId,
                context.Message.CustomerId,
            };

            await customerValidationEngineProducer
                .Produce(customerValidationEvent);

            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderRequestState, OrderRequestEvent, TException> context, 
            IBehavior<OrderRequestState, OrderRequestEvent> next) 
            where TException : Exception
        {
            await next.Execute(context)
                .ConfigureAwait(false);
        }

        public void Probe(ProbeContext context) => context.CreateScope("order-placed");
        public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
        
    }
}
