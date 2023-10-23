using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.Consumers
{
    public class OrderManagementSystemConsumer : IConsumer<OrderRequestEvent>
    {
        private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationResponseEvent;
        private readonly IApiService apiService;

        public OrderManagementSystemConsumer(
            ITopicProducer<CustomerValidationRequestEvent> customerValidationResponseEvent, 
            IApiService apiService)
        {
            this.customerValidationResponseEvent = customerValidationResponseEvent;
            this.apiService = apiService;
        }

        public async Task Consume(ConsumeContext<OrderRequestEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            // calling some external service before producing data and go to the next step of out state machine
            await apiService
                .SomeApiCallAsync()
                .ConfigureAwait(false);
            
            var customerValidationRequest = new CustomerValidationRequestEvent
            {
                CorrelationId = context.Message.CorrelationId,
                CustomerId = context.Message.CustomerId,
            };
            
            await customerValidationResponseEvent
                .Produce(customerValidationRequest);
        }
    }
}
