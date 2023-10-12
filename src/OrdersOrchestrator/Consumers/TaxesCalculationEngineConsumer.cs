using MassTransit;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class TaxesCalculationEngineConsumer : IConsumer<TaxesCalculationResponse>
    {
        public TaxesCalculationEngineConsumer()
        {
        }

        public Task Consume(ConsumeContext<TaxesCalculationResponse> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            
            return Task.CompletedTask;
        }
    }
}
