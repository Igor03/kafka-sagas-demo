using MassTransit;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.OrderManagement;

namespace OrdersOrchestrator.Consumers
{
    public class DeadLetterConsumer : IConsumer<DeadLetterMessage>
    {
        public Task Consume(ConsumeContext<DeadLetterMessage> context)
        {
            return Task.CompletedTask;
        }
    }
}
