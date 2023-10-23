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

        public async Task Consume(ConsumeContext<CustomerValidationResponseEvent> context)
        {
            try
            {
                throw new ArgumentException(nameof(context.Message.CustomerType));
            }
            catch
            {
                var message = new DeadLetterMessage
                {
                    CorrelationId = context.Message.CorrelationId,
                    Message = context.Message
                };

                await DeadLetterProducer.Produce(message);
            }
           
        }
    }
}
