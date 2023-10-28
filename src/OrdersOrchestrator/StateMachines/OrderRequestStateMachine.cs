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

        Initially(
            When(OrderRequestedEvent)
                .InitializeSaga()
                .Activity(config => config.OfType<ReceiveOrderRequestStepActivity>())
                .TransitionTo(ValidatingCustomer)
                .Catch<EventExecutionException>(callback => callback.TransitionTo(Faulted)
                    // In case we need process more complex fault compensation routines
                    .Activity(c => c.OfType<ReceiveOrderRequestStepActivity>())), 
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

        During(Faulted,
            When(FaultEvent)
                .NotifySourceSystem()
                .Finalize());

        WhenEnter(FinishedOrder, x => x.Then(
            context => LogContext.Info?.Log("Order management system notified: {0}",
            context.Saga.CorrelationId)).TransitionTo(SourceSystemNotified).Finalize());

        // Delete finished saga instances from the repository
        // SetCompletedWhenFinalized();
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
        Event(() => FaultEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            configurator.OnMissingInstance(m => m.Discard());
        });
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