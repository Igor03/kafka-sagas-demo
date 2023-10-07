using Contracts;
using Contracts.Exceptions;
using MassTransit;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class ReceiveOrderRequestActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>
{
    private readonly IApiService apiService;
    private readonly ITopicProducer<string, CustomerValidationRequestEvent> customerValidationEngineProducer;

    public ReceiveOrderRequestActivity(
        IApiService apiService, 
        ITopicProducer<string, CustomerValidationRequestEvent> customerValidationEngineProducer)
    {
        this.apiService = apiService;
        this.customerValidationEngineProducer = customerValidationEngineProducer;
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>.Execute(
        BehaviorContext<OrderRequestSagaInstance, OrderRequestEvent> context, 
        IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next)
    {
        // Some dummy validation to test the error handling flow
        if (await apiService
            .ValidateIncomingOrderRequestAsync(context.Message)
            .ConfigureAwait(false))
        {
            throw new NotATransientException("Error during order request validation!");
        }

        var customerValidationEvent = new
        {
            context.Message.CustomerId,
        };

        await customerValidationEngineProducer
            .Produce(
                Guid.NewGuid().ToString(),
                customerValidationEvent,
                context.CancellationToken)
            .ConfigureAwait(false);

        await next.Execute(context)
            .ConfigureAwait(false);
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, OrderRequestEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next) 
        => await next.Faulted(context).ConfigureAwait(false);
    
    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(ReceiveOrderRequestActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    
}

