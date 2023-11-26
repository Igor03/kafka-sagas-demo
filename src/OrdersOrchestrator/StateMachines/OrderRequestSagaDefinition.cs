using Contracts.Exceptions;
using MassTransit;
using MassTransit.Middleware;
using MassTransit.Transports;
using OrdersOrchestrator.Middlewares;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestSagaDefinition : SagaDefinition<OrderRequestSagaInstance>
{
    public OrderRequestSagaDefinition()
    {
        // Processing up to 5 messages at time
        ConcurrentMessageLimit = 5;
    }
    
    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator, 
        ISagaConfigurator<OrderRequestSagaInstance> sagaConfigurator,
        IRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(endpointConfigurator, nameof(endpointConfigurator));
        ArgumentNullException.ThrowIfNull(sagaConfigurator, nameof(sagaConfigurator));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        
        // Retrying unhandled exceptions by the saga state machine
        sagaConfigurator.UseMessageRetry(config => 
        {
            config.Interval(3, 1000);
            config.Ignore<NotATransientException>();
        });
        
        // Configuring a filter for all the registered events in the state machine
        sagaConfigurator.UseFilter(new SagaLoggingMiddlewareFilter<OrderRequestSagaInstance>());
        
        endpointConfigurator.ConfigureError(x =>
        {
            x.UseFilters(
                new FaultProcessingMiddlewareFilter(context.GetRequiredService<IErrorTransport>()),
                new ErrorTransportFilter());
        });
        
        base.ConfigureSaga(endpointConfigurator, sagaConfigurator, context);
    }
}