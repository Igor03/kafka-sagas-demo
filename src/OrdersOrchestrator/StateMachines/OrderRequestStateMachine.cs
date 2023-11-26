using Contracts;
using MassTransit;
using OrdersOrchestrator.StateMachines.CustomActivities;

namespace OrdersOrchestrator.StateMachines;

public sealed class OrderRequestStateMachine 
    : MassTransitStateMachine<OrderRequestSagaInstance>
{
    public OrderRequestStateMachine()
    {
        // Registering known states
        RegisterStates();
        // Registering and correlating events
        CorrelateEvents();
        
        Initially(
            When(OrderRequestedEvent)
                .InitializeSaga()
                .Activity(config => config.OfType<ReceiveOrderRequestActivity>())
                .TransitionTo(ValidatingCustomer),
            When(FaultEvent)
                .Activity(config => config.OfType<ProcessFaultActivity>())
                .TransitionTo(Faulted));

        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
                .UpdateSaga()
                .Activity(config => config.OfType<CustomerValidationActivity>())
                .TransitionTo(CalculatingTaxes),
            When(FaultEvent)
                .Activity(config => config.OfType<ProcessFaultActivity>())
                .TransitionTo(Faulted));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .UpdateSaga()
                .Activity(config => config.OfType<TaxesCalculationActivity>())
                .TransitionTo(FinishedOrder),
            When(FaultEvent)
                .Activity(config => config.OfType<ProcessFaultActivity>())
                .TransitionTo(Faulted));
        
        WhenEnter(Faulted,
            context => context.TransitionTo(NotifyingSourceSystem));

        WhenEnter(FinishedOrder,
            activityCallback => activityCallback
                .NotifySourceSystem()
                .Then(context => LogContext.Info?.Log("Order management system notified: {0}", context.CorrelationId))
                .Finalize());

        During(Final,
            Ignore(OrderRequestedEvent),
            Ignore(CustomerValidationResponseEvent),
            Ignore(TaxesCalculationResponseEvent),
            Ignore(FaultEvent));
        
        // Delete finished saga instances from the repository
        // SetCompletedWhenFinalized();
    }

    private void CorrelateEvents()
    {
        Event(() => OrderRequestedEvent, x => x
            .CorrelateById(m => m.CorrelationId ?? new Guid())
            .SelectId(m => m.CorrelationId ?? new Guid())
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => CustomerValidationResponseEvent, x => x
            .CorrelateById(m => m.CorrelationId ?? new Guid())
            .SelectId(m => m.CorrelationId ?? new Guid())
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => TaxesCalculationResponseEvent, x => x
            .CorrelateById(m => m.CorrelationId ?? new Guid())
            .SelectId(m => m.CorrelationId ?? new Guid())
            .OnMissingInstance(m => m.Discard()));
        
        Event(() => FaultEvent, x => x
            .CorrelateById(m => m.CorrelationId ?? new Guid())
            .SelectId(m => m.CorrelationId ?? new Guid())
            .OnMissingInstance(m => m.Discard()));
    }

    private void RegisterStates() 
        => InstanceState(x => x.CurrentState);
    
    public State? ValidatingCustomer { get; set; }
    public State? CalculatingTaxes { get; set; }
    public State? FinishedOrder { get; set; }
    public State? NotifyingSourceSystem { get; set; }
    public State? Faulted { get; set; }
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; set; }
    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; set; }
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; set; }
    public Event<FaultMessageEvent>? FaultEvent { get; set; }
}