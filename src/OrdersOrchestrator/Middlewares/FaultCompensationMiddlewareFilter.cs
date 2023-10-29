using MassTransit;

namespace OrdersOrchestrator.Middlewares
{
    public sealed class FaultCompensationMiddlewareFilter<TSaga> 
        : IFilter<SagaConsumeContext<TSaga>>
        where TSaga : class, ISaga
    {
        public Task Send(SagaConsumeContext<TSaga> context, IPipe<SagaConsumeContext<TSaga>> next)
        {
            throw new NotImplementedException();
        }

        public void Probe(ProbeContext context) 
            => context.CreateFilterScope("compensation-step");
    }
}
