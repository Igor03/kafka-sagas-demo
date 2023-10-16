using MassTransit;
using OrdersOrchestrator.Contracts.ApiService;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.Consumers
{
    public class OrderManagementSystemConsumer : IConsumer<OrderRequestEvent>
    {
        private readonly ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer;
        private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationResponseEvent;
        private readonly IApiService apiService;

        public OrderManagementSystemConsumer(
            ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer, 
            ITopicProducer<CustomerValidationRequestEvent> customerValidationResponseEvent, 
            IApiService apiService)
        {
            this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
            this.customerValidationResponseEvent = customerValidationResponseEvent;
            this.apiService = apiService;
        }

        public async Task Consume(ConsumeContext<OrderRequestEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            if (await apiService.ValidateRequestAsync(context.Message.CustomerId) is ApiServiceResponse response)
            {
                var customerValidationRequest = new CustomerValidationRequestEvent
                {
                    CustomerId = context.Message.CustomerId,
                    CorrelationId = context.Message.CorrelationId,
                };

                await customerValidationResponseEvent
                    .Produce(customerValidationRequest);
            }
        }
    }
}
