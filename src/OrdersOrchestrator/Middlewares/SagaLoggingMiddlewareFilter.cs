using MassTransit;
using Newtonsoft.Json;

namespace OrdersOrchestrator.Middlewares;

public sealed class SagaLoggingMiddlewareFilter<TSaga> : IFilter<SagaConsumeContext<TSaga>>
    where TSaga : class, ISaga
{
    public async Task Send(SagaConsumeContext<TSaga> context, IPipe<SagaConsumeContext<TSaga>> next)
    {
        // LogContext.Info?.Log(
        //     JsonConvert.SerializeObject(context.Saga, Formatting.Indented));
        
        await next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateScope("saga-logging");
}