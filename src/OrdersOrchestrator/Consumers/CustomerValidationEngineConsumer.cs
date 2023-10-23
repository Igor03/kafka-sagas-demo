using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class CustomerValidationEngineConsumer : IConsumer<CustomerValidationResponseEvent>
    {
        public Task Consume(ConsumeContext<CustomerValidationResponseEvent> context)
        {
            return Task.CompletedTask;
        }
    }
}
