using Contracts;
using MassTransit;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class ReceiveOrderRequestStepActivity : IStateMachineActivity<OrderRequestSagaInstance, OrderRequestEvent>
{
    private readonly IApiService apiService;
    private readonly ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer;
    private readonly ITopicProducer<ErrorMessageEvent> errorProducer;

    public ReceiveOrderRequestStepActivity(
        IApiService apiService, 
        ITopicProducer<CustomerValidationRequestEvent> customerValidationEngineProducer, 
        ITopicProducer<ErrorMessageEvent> errorProducer)
    {
        this.apiService = apiService;
        this.customerValidationEngineProducer = customerValidationEngineProducer;
        this.errorProducer = errorProducer;
    }

    public async Task Execute(
        BehaviorContext<OrderRequestSagaInstance, OrderRequestEvent> context, 
        IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next)
    {
        // Some dummy validation to test the error handling flow
        if (await apiService
            .ValidateIncomingOrderRequestAsync(context.Message)
            .ConfigureAwait(false))
        {
            // Not a transient exception...
            throw new Exception("Error during order request validation!. Not a transient exception");
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

    public async Task Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, OrderRequestEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, OrderRequestEvent> next) 
        where TException : Exception
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
    
    public void Probe(ProbeContext context) => context.CreateScope("order-placed");
    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    
}

