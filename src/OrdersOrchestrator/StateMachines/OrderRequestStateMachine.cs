using MassTransit;
using OrdersOrchestrator.Activities;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestSagaInstance>
{
    public OrderRequestStateMachine(IConfiguration configuration)
    {
        // Registering known states
        RegisterStates();
        // Registering and correlating events
        CorrelateEvents();

        var maxRetriesAttempts =
            configuration.GetSection("KafkaOptions:Topics").GetValue<short>("MaxRetriesAttempts");
        
        Initially(
            When(OrderRequestedEvent)
                .InitializeSaga()
                .Activity(config => config.OfType<ReceiveOrderRequestStepActivity>())
                .TransitionTo(ValidatingCustomer)
                .Catch<EventExecutionException>(callback => callback.TransitionTo(Faulted)
                    // In case we need to process more complex fault compensation routines
                    .Activity(compCallback => compCallback.OfType<ReceiveOrderRequestStepActivity>())),
            Ignore(FaultEvent));

        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
              .UpdateSaga()
              .Activity(config => config.OfType<CustomerValidationStepActivity>())
              .TransitionTo(CalculatingTaxes)
              .Catch<EventExecutionException>(callback => callback.TransitionTo(Faulted)
                  // Simply produces a new error event to the error topic
                .SendErrorEvent()), 
            Ignore(FaultEvent));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .UpdateSaga()
                .Activity(config => config.OfType<TaxesCalculationStepActivity>())
                .TransitionTo(FinishedOrder)
                .Catch<EventExecutionException>(callback => callback.TransitionTo(Faulted)
                    // Simply produces a new error event to the error topic
                    .SendErrorEvent()), 
            Ignore(FaultEvent));

        
        // TODO: Add a schedule to delay messages redeliveries following this doc: https://masstransit.io/documentation/patterns/saga/state-machine#schedule
        During(Faulted,
            When(FaultEvent)
                // We can create more complex logic here to decide whether the message should be retried/redelivered
                .IfElse(context => context.Saga.Attempts < maxRetriesAttempts,
                    i => i.TransitionTo(Initial).RestartProcess(),
                    e => e.NotifySourceSystem().TransitionTo(FinishedOrder)));
        
        During(Final, Ignore(FaultEvent));

        WhenEnter(FinishedOrder, x => x.Then(
            context => LogContext.Info?.Log("Order management system notified: {0}",
            context.Saga.CorrelationId)).TransitionTo(SourceSystemNotified).Finalize());

        // Delete finished saga instances from the repository
        SetCompletedWhenFinalized();
    }

    private void CorrelateEvents()
    {
        Event(() => OrderRequestedEvent, x => x
            .CorrelateById(m => m.Message.CorrelationId)
            .SelectId(m => m.Message.CorrelationId)
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => CustomerValidationResponseEvent, x => x
            .CorrelateById(m => m.Message.CorrelationId)
            .SelectId(m => m.Message.CorrelationId)
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => TaxesCalculationResponseEvent, x => x
            .CorrelateById(m => m.Message.CorrelationId)
            .SelectId(m => m.Message.CorrelationId)
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => FaultEvent, x => x
            .CorrelateById(m => m.Message.CorrelationId)
            .SelectId(m => m.Message.CorrelationId)
            .OnMissingInstance(m => m.Discard()));
    }

    private void RegisterStates() 
        => InstanceState(x => x.CurrentState);
    
    public State? ValidatingCustomer { get; set; }
    public State? CalculatingTaxes { get; set; }
    public State? FinishedOrder { get; set; }
    public State? SourceSystemNotified { get; set; }
    public State? Faulted { get; set; }
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; set; }
    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; set; }
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; set; }
    public Event<ErrorMessageEvent>? FaultEvent { get; set; }
}