using Contracts;
using MassTransit;
using MassTransit.Transports;

namespace OrdersOrchestrator.Transports;

public sealed class FaultTransport : IErrorTransport
{
    async Task IErrorTransport.Send(ExceptionReceiveContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        
        var consumeContext = context.GetPayload<ConsumeContext>();
        
        var faultTopicProducer = consumeContext
            .GetServiceOrCreateInstance<ITopicProducer<string, FaultMessageEvent>>();
        
        var canReadMessage = consumeContext
            .TryGetMessage<object>(out var message);

        var faultEvent = new FaultMessageEvent
        {
            Message = canReadMessage ? message : null,
            Exception = context.Exception,
        };
        
        await faultTopicProducer
            .Produce(
                Guid.NewGuid().ToString(),
                faultEvent,
                Pipe.Execute<KafkaSendContext>(p =>
                {
                    p.CorrelationId = consumeContext.CorrelationId;
                    p.Headers.SetHostHeaders();
                }),
                context.CancellationToken)
            .ConfigureAwait(false);
    }
}