using MassTransit;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class OrderManagementSystemConsumer : IConsumer<OrderRequestEvent>
    {
        private readonly ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer;

        public OrderManagementSystemConsumer(ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer)
        {
            this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
        }

        public async Task Consume(ConsumeContext<OrderRequestEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var taxesCalculationRequest = new TaxesCalculationRequestEvent
            {
                ItemId = context.Message.ItemId,
                CorrelationId = context.Message.CorrelationId,
            };
            
            // await taxesCalculationEngineProducer
            //     .Produce(taxesCalculationRequest);
        }
    }
}
