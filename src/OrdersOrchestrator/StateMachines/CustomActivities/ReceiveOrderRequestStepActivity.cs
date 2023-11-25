using Contracts;
using Contracts.Exceptions;
using MassTransit;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class ReceiveOrderRequestStepActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>
{
    private readonly IApiService apiService;
    private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer;
    private readonly ITopicProducer<FaultMessageEvent> errorProducer;

    public ReceiveOrderRequestStepActivity(
        IApiService apiService, 
        ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer, 
        ITopicProducer<FaultMessageEvent> errorProducer)
    {
        this.apiService = apiService;
        this.customerValidationEngineProducer = customerValidationEngineProducer;
        this.errorProducer = errorProducer;
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
            .Produce(customerValidationEvent);

        await next.Execute(context)
            .ConfigureAwait(false);
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, OrderRequestEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next) 
    {
        // Updating saga with useful information about the current state
        context.Saga.Reason = context.Exception.Message;
        context.Saga.UpdatedAt = DateTime.Now;
        
        LogContext.Info?.Log("Processing fault for {0}: {1}", context.Event.Name, context.Saga.CorrelationId);
        
        // In case we need to process more complex fault compensation routines
        var errorEvent = new
        {
            context.Saga.CorrelationId,
            context.Message,
            // Here we can use a Pipe<>
            __Header_Reason = context.Exception.Message,
        };
            
        await errorProducer
            .Produce(errorEvent)
            .ConfigureAwait(false);

        await next.Execute(context)
            .ConfigureAwait(false);
    }
    
    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(ReceiveOrderRequestStepActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    
}

