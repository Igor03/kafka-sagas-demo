using MassTransit;
using Orchestrator.Contracts;

namespace Orchestrator.Consumers
{
    public class SourceSystemRequestConsumer : IConsumer<SourceSystemRequest>
    {
        private readonly ITopicProducer<string, ConsumerXRequest> _consumerXProducer;

        public SourceSystemRequestConsumer(ITopicProducer<string, ConsumerXRequest> producer)
        {
            _consumerXProducer = producer;
        }

        public async Task Consume(ConsumeContext<SourceSystemRequest> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            
            var consumerXRequestMessage = new ConsumerXRequest(context.Message.Operation);

            await _consumerXProducer
                .Produce(context.GetKey<string>(), consumerXRequestMessage);
        }
    }
}
