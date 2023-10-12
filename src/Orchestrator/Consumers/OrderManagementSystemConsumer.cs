using MassTransit;
using Orchestrator.Contracts.OrderManagement;
using Orchestrator.Contracts.TaxesCalculationEngine;

namespace Orchestrator.Consumers
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
