using MassTransit;
using OrdersOrchestrator.Configuration;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestState>
{
    public OrderRequestStateMachine(IConfiguration configuration)
    {
        InstanceState(x => x.CurrentState, Placed);
        
        // Registering and correlating events
        CorrelateEvents();
        // ConfigureRetry();

        Initially(
            When(OrderRequestedEvent)
                .Then(context => LogContext.Info?.Log("Receiving order request: {0}", context.Saga.CorrelationId))
                .InitializeSaga()
                .LogSaga()
                .TransitionTo(ValidatingCustomer));
        
        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
              .Then(context => LogContext.Info?.Log("Validating customer: {0}", context.Saga.CorrelationId))
              .SendToTaxesCalculation()
              .LogSaga()
              .TransitionTo(CalculatingTaxes));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(context => LogContext.Info?.Log("Calculating taxes: {0}", context.Saga.CorrelationId))
                .ReplyToRequestingSystem()
                .LogSaga()
                .TransitionTo(FinishedOrder),
             When(DeadLetterRetryEvent)
                .ProcessRetry()
                .TransitionTo(Faulted));


        During(Faulted,
            When(OrderRequestedEvent)
            .TransitionTo(ValidatingCustomer));
        
        WhenEnter(FinishedOrder, x => x.Then(context => LogContext.Info?.Log("Order replyied taxes: {0}", context.Saga.CorrelationId)));

        var retryCount = configuration
            .GetSection("KafkaOptions:Topics")
            .GetValue<short>("MaxRetriesAttemps");
        
        var retryDelay = TimeSpan.FromSeconds(10);

        //WhenEnter(Faulted, x => x
        //    .If(context => context.Saga.RetryAttempt < retryCount,
        //        retry => retry
        //            .Schedule(RetryDelayExpired, context => new RetryDelayExpired(context.Saga.CorrelationId), _ => retryDelay)
        //            .TransitionTo(WaitingToRetry)
        //    )
        //);

        // Marking as completed for the SM instance to be removed from the repository
        SetCompletedWhenFinalized();
    }

    private void CorrelateEvents()
    {
        Event(() => OrderRequestedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CustomerValidationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => TaxesCalculationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        // Event(() => DeadLetterRetryEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    }

    private void ConfigureRetry()
    {
        Schedule(() => RetryDelayExpired, saga => saga.ScheduleRetryToken, x =>
        {
            x.Received = r =>
            {
                r.CorrelateById(context => context.Message.CorrelationId);
                r.ConfigureConsumeTopology = false;
            };
        });
    }

    public State? Placed { get; set; }
    public State? ValidatingCustomer { get; set; }
    public State? CalculatingTaxes { get; set; }
    public State? FinishedOrder { get; set; }
    public State? Faulted { get; set; }
    public State? WaitingToRetry { get; set; }
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; set; }
    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; set; }
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; set; }
    public Event<DeadLetterMessage>? DeadLetterRetryEvent { get; set; }
    public Schedule<OrderRequestState, RetryDelayExpired> RetryDelayExpired { get; set; }

}