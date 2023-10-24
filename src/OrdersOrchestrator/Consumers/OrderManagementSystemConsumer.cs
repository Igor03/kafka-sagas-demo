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

            if (await this.apiService.ValidateIncomingRequestAsync(context.Message))
                throw new ArgumentException("Something wrong just happened");
        }
    }
}
