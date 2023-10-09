using ConsumerX.Contracts;
using MassTransit;

namespace ConsumerX.Consumers
{
    public class XRequestConsumer : IConsumer<ConsumerXRequest>
    {
        private readonly ITopicProducer<string, ConsumerXResponse> _responseProducer;
   
        public XRequestConsumer(ITopicProducer<string, ConsumerXResponse> _responseProducer)
        {
            this._responseProducer = _responseProducer;
        }

        public async Task Consume(ConsumeContext<ConsumerXRequest> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var response = new ConsumerXResponse(true, DateTime.Now);

            await _responseProducer
                .Produce(context.GetKey<string>(), response)
                .ConfigureAwait(false);
        }
    }
}
