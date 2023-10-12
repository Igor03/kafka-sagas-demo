using MassTransit;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class OrderManagementSystemConsumer : IConsumer<OrderRequest>
    {
        private readonly ITopicProducer<string, TaxesCalculationRequest> taxesCalculationEngineProducer;

        public OrderManagementSystemConsumer(ITopicProducer<string, TaxesCalculationRequest> taxesCalculationEngineProducer)
        {
            this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
        }

        public async Task Consume(ConsumeContext<OrderRequest> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var taxesCalculationRequest = new TaxesCalculationRequest
            {
                ItemId = context.Message.ItemId,
            };
            
            await taxesCalculationEngineProducer
                .Produce(context.GetKey<string>(), taxesCalculationRequest);
        }
    }
}
