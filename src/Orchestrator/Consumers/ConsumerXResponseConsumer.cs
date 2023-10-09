using MassTransit;
using Orchestrator.Contracts;

namespace Orchestrator.Consumers
{
    public class ConsumerXResponseConsumer : IConsumer<ConsumerXResponse>
    {
        private readonly ITopicProducer<string, SourceSystemResponse> _sourceSystemResponseProducer;

        public ConsumerXResponseConsumer(ITopicProducer<string, SourceSystemResponse> sourceSystemResponseProducer)
        {
            _sourceSystemResponseProducer = sourceSystemResponseProducer;
        }

        public async Task Consume(ConsumeContext<ConsumerXResponse> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var sourceSystemResponse = new SourceSystemResponse(context.Message.Response, DateTime.Now);

            await _sourceSystemResponseProducer
                .Produce(context.GetKey<string>(), sourceSystemResponse)
                .ConfigureAwait(false);
        }
    }
}
