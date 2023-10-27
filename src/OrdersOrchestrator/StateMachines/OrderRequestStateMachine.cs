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
              .Catch<Exception>(p => p.TransitionTo(Faulted)
                .Activity(x => x.OfType<CustomerValidationStepActivity>())));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(context => LogContext.Info?.Log("Taxes calculation: {0}", context.Saga.CorrelationId))
                .Activity(config => config.OfType<TaxesCalculationStepActivity>().TransitionTo(Faulted))
                .TransitionTo(FinishedOrder)
                .Catch<Exception>(p => p.TransitionTo(Faulted)
                    // Sending message to error topic outside of an activity
                    .SendToErrorTopic()));


        // During(Faulted,
        //    When(DeadLetterRetryEvent)
        //         .Then(_ => 
        //         {
        //             LogContext.Info?.Log("DeadLetter stuff");
        //         }).TransitionTo(WaitingToRetry));
        
        During(Faulted,
            When(DeadLetterRetryEvent)
                .Activity(config => config.OfType<ProcessFaultedMessageStepActivity>())
                .Then(context => LogContext.Info?.Log("Message from error topic", context.Saga.CorrelationId)));


        WhenEnter(FinishedOrder, x => x.Then(
            context => LogContext.Info?.Log("Order management system notified: {0}",
            context.Saga.CorrelationId)).TransitionTo(SourceSystemNotified));

        // Delete finished saga instances from the database
        SetCompletedWhenFinalized();
    }

    private void CorrelateEvents()
    {
        Event(() => OrderRequestedEvent, configurator => 
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            // configurator.OnMissingInstance(m => m.Discard());
        });
        Event(() => CustomerValidationResponseEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            // configurator.OnMissingInstance(m => m.Discard());
        });
        Event(() => TaxesCalculationResponseEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            // configurator.OnMissingInstance(m => m.Discard());
        });
        Event(() => DeadLetterRetryEvent, configurator =>
        {
            configurator.CorrelateById(context => context.Message.CorrelationId);
            // configurator.OnMissingInstance(m => m.Discard());
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
    public Event<ErrorMessageEvent>? DeadLetterRetryEvent { get; set; }
}