using MassTransit;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class TaxesCalculationEngineConsumer : IConsumer<TaxesCalculationResponseEvent>
    {
        public async Task Consume(ConsumeContext<TaxesCalculationResponseEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            
            // return Task.CompletedTask;
        }
    }
}
