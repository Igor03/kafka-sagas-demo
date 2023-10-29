using MassTransit;
using MassTransit.Middleware;
using OrdersOrchestrator.Middlewares;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestSagaDefinition : SagaDefinition<OrderRequestSagaInstance>
{
    public OrderRequestSagaDefinition(IConfiguration configuration)
    {
        // Processing up to 5 messages at time
        ConcurrentMessageLimit = 1;
    }
    
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderRequestSagaInstance> sagaConfigurator,
        IRegistrationContext context)
    {
        // Retrying unhandled exceptions by the saga state machine
        sagaConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
        
        // Configuring a filter for all the registered events in the state machine
        sagaConfigurator.UseFilter(new SagaLoggingMiddlewareFilter<OrderRequestSagaInstance>());
        
        endpointConfigurator.ConfigureError(x =>
        {
            x.UseFilter(new ErrorTransportFilter());
        });
        
        base.ConfigureSaga(endpointConfigurator, sagaConfigurator, context);
    }
}