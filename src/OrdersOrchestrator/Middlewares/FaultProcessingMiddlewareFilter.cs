using MassTransit;
using MassTransit.Transports;

namespace OrdersOrchestrator.Middlewares;

public sealed class FaultProcessingMiddlewareFilter 
    : IFilter<ExceptionReceiveContext>
{
    private readonly IErrorTransport errorTransport;
    
    public FaultProcessingMiddlewareFilter(IErrorTransport errorTransport)
    {
        this.errorTransport = errorTransport;
    }

    async Task IFilter<ExceptionReceiveContext>
        .Send(ExceptionReceiveContext context, IPipe<ExceptionReceiveContext> next)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(next, nameof(next));

        context.AddOrUpdatePayload(
            () => errorTransport,
            _ => errorTransport);

        await next.Send(context)
            .ConfigureAwait(false);
    }
    
    void IProbeSite.Probe(ProbeContext context)
        => context.CreateScope(nameof(FaultProcessingMiddlewareFilter));
}