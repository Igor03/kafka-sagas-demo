using MassTransit;

namespace OrdersOrchestrator.Middlewares
{
    public sealed class FaultCompensationMiddlewareFilter : IFilter<ExceptionReceiveContext>
    {
        public void Probe(ProbeContext context) 
            => context.CreateFilterScope("compensation-step");


        public async Task Send(ExceptionReceiveContext context, IPipe<ExceptionReceiveContext> next)
        {
            await next.Send(context)
                .ConfigureAwait(false);
        }
    }
}
