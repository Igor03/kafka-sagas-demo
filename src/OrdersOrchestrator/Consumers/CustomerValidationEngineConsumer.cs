using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;

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
