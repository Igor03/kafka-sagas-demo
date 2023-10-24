using MassTransit;
using OrdersOrchestrator.Activities;
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

        Initially(
            When(OrderRequestedEvent)
                .InitializeSaga()
                .Then(context => LogContext.Info?.Log("Validating Customer: {0}", context.Saga.CorrelationId))
                .Activity(x => x.OfType<ReceiveOrderRequestStepActivity>())
                .Catch<Exception>(x => x.Then(p =>
                {
                    LogContext.Info?.Log(p.Saga.CurrentState.ToString());
                }))
                .TransitionTo(ValidatingCustomer));
        
        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
              .Then(context => LogContext.Info?.Log("Calculating Taxes: {0}", context.Saga.CorrelationId))
              .Activity(x => x.OfType<CustomerValidationStepActivity>())
              .SendToTaxesCalculation()
              .LogSaga()
              .TransitionTo(CalculatingTaxes));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(context => LogContext.Info?.Log("Replying to Source System", context.Saga.CorrelationId))
                .ReplyToRequestingSystem().LogSaga()
                .TransitionTo(FinishedOrder));

        During(Faulted,
            When(CustomerValidationResponseEvent)
            .Then(x =>
            {
                LogContext.Info?.Log("Testintg");
            })
            .Activity(x => x.OfType<CustomerValidationStepActivity>())
            .TransitionTo(ValidatingCustomer));
        
        WhenEnter(FinishedOrder, x => x.Then(context => LogContext.Info?.Log("Order replyied: {0}", context.Saga.CorrelationId)));

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
        Event(() => OrderRequestedEvent, configurator => 
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            configurator.OnMissingInstance(m => m.Discard());
        });
        Event(() => CustomerValidationResponseEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            configurator.OnMissingInstance(m => m.Discard());
        });
        Event(() => TaxesCalculationResponseEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            configurator.OnMissingInstance(m => m.Discard());
        });
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