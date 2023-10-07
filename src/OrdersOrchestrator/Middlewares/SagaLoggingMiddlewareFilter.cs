using MassTransit;

namespace OrdersOrchestrator.Middlewares;

public sealed class SagaLoggingMiddlewareFilter<TSaga> 
    : IFilter<SagaConsumeContext<TSaga>>
    where TSaga : class, ISaga
{
    async Task IFilter<SagaConsumeContext<TSaga>>.Send(
        SagaConsumeContext<TSaga> context, 
        IPipe<SagaConsumeContext<TSaga>> next)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(next, nameof(next));
        
        await next
            .Send(context)
            .ConfigureAwait(false);
    }
    
    void IProbeSite.Probe(ProbeContext context) 
        => context.CreateScope(nameof(SagaLoggingMiddlewareFilter<TSaga>));
}