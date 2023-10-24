using MassTransit;

namespace OrdersOrchestrator.Middlewares
{
    public sealed class LoggingMiddlewareFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
        where TMessage : class
    {
        public void Probe(ProbeContext context) => context.CreateFilterScope("Logging");

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            await next.Send(context).ConfigureAwait(false);
        }
    }
}
