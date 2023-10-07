using Contracts;
using MassTransit;
using MassTransit.Transports;

namespace OrdersOrchestrator.Transports;

public sealed class FaultTransport : IErrorTransport
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    
    public FaultTransport(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }
    async Task IErrorTransport.Send(ExceptionReceiveContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var consumeContext = context.GetPayload<ConsumeContext>();

        using var scope = serviceScopeFactory.CreateScope();
        var faultTopicProducer = scope
            .ServiceProvider
            .GetRequiredService<ITopicProducer<string, FaultMessageEvent>>();
        
        consumeContext.TryGetMessage<object>(out var incomingConsume);

        var faultEvent = new FaultMessageEvent
        {
            Event = incomingConsume?.Message,
            ExceptionMessage = context.Exception.InnerException?.Message
        };
        
        await faultTopicProducer
            .Produce(
                Guid.NewGuid().ToString(),
                faultEvent,
                Pipe.Execute<KafkaSendContext>(p =>
                {
                    p.CorrelationId = consumeContext.CorrelationId;
                    p.Headers.SetExceptionHeaders(context);
                }),
                context.CancellationToken)
            .ConfigureAwait(false);
    }
}