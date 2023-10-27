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
                .Then(context => LogContext.Info?.Log("Order requested: {0}", context.Saga.CorrelationId))
                .Activity(config => config.OfType<ReceiveOrderRequestStepActivity>())
                .TransitionTo(ValidatingCustomer)
                .Catch<EventExecutionException>(p => p.TransitionTo(Faulted)
                    .Activity(x => x.OfType<ReceiveOrderRequestStepActivity>())));

        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
              .Then(context => LogContext.Info?.Log("Customer validation: {0}", context.Saga.CorrelationId))
              .Activity(config => config.OfType<CustomerValidationStepActivity>())
              .TransitionTo(CalculatingTaxes)
              .Catch<EventExecutionException>(p => p.TransitionTo(Faulted)
                .Activity(x => x.OfType<CustomerValidationStepActivity>())));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(context => LogContext.Info?.Log("Taxes calculation: {0}", context.Saga.CorrelationId))
                .Activity(config => config.OfType<TaxesCalculationStepActivity>())
                .TransitionTo(FinishedOrder)
                .Catch<EventExecutionException>(p => p.TransitionTo(Faulted)
                    .Then(p => LogContext.Info?.Log("IGOR: {0}", p.Saga.CurrentState))
                    .SendErrorEvent()));

        During(CalculatingTaxes,
            Ignore(FaultEvent));

        During(Faulted,
            When(FaultEvent)
                .Activity(config => config.OfType<ProcessFaultedMessageStepActivity>())
                .Then(context => LogContext.Info?.Log("Processing compensation for: {0}", context.Saga.CorrelationId))
                .Finalize());

        WhenEnter(FinishedOrder, x => x.Then(
            context => LogContext.Info?.Log("Order management system notified: {0}",
            context.Saga.CorrelationId)).TransitionTo(SourceSystemNotified).Finalize());

        // Delete finished saga instances from the database
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
        Event(() => FaultEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            configurator.OnMissingInstance(m => m.Discard());
        });
    }

    private void RegisterStates()
    {
        InstanceState(x => x.CurrentState);
    }

    public State? Placed { get; set; }
    public State? ValidatingCustomer { get; set; }
    public State? CalculatingTaxes { get; set; }
    public State? FinishedOrder { get; set; }
    public State? SourceSystemNotified { get; set; }
    public State? Faulted { get; set; }
    public State? WaitingToRetry { get; set; }
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; set; }
    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; set; }
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; set; }
    public Event<ErrorMessageEvent>? FaultEvent { get; set; }
}