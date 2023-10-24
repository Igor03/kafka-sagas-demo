using MassTransit;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Consumers
{
    public class CustomerValidationEngineConsumer : IConsumer<CustomerValidationResponseEvent>
    {
        private readonly ITopicProducer<DeadLetterMessage> DeadLetterProducer;

        public CustomerValidationEngineConsumer(ITopicProducer<DeadLetterMessage> deadLetterProducer)
        {
            DeadLetterProducer = deadLetterProducer;
        }

        public Task Consume(ConsumeContext<CustomerValidationResponseEvent> context)
        {
            return Task.CompletedTask;
        }
    }
}
