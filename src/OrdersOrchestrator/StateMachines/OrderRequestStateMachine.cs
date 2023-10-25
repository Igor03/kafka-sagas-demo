using MassTransit;
using OrdersOrchestrator.Activities;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestState>
{
    public OrderRequestStateMachine(IConfiguration configuration)
    {
        InstanceState(x => x.CurrentState, 
            Placed, 
            ValidatingCustomer, 
            CalculatingTaxes, 
            FinishedOrder, 
            Faulted, 
            WaitingToRetry);
        
        // Registering and correlating events
        CorrelateEvents();

        Initially(
            When(OrderRequestedEvent)
                .InitializeSaga()
                .Then(context => LogContext.Info?.Log("Validating Customer: {0}", context.Saga.CorrelationId))
                .Activity(x => x.OfType<ReceiveOrderRequestStepActivity>())
                .Catch<Exception>(p => p.Activity(x => x.OfType<ExceptionCompensationStepActivity<OrderRequestEvent>>()))
                .TransitionTo(ValidatingCustomer));

        During(ValidatingCustomer,
            When(CustomerValidationResponseEvent)
              .Then(context => LogContext.Info?.Log("Calculating Taxes: {0}", context.Saga.CorrelationId))
              .Activity(x => x.OfType<CustomerValidationStepActivity>())
              .TransitionTo(CalculatingTaxes));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(context => LogContext.Info?.Log("Replying to Source System {0}", context.Saga.CorrelationId))
                .Activity(x => x.OfType<TaxesCalculationStepActivity>())
                .TransitionTo(FinishedOrder));


        During(Faulted,
           When(OrderRequestedEvent)
               .Then(_ => LogContext.Info?.Log("Compensating for 1"))
               .TransitionTo(WaitingToRetry),
           When(CustomerValidationResponseEvent)
               .Then(_ => LogContext.Info?.Log("Compensating for 2"))
               .TransitionTo(WaitingToRetry),
           When(TaxesCalculationResponseEvent)
               .Then(_ => LogContext.Info?.Log("Compensating for 3"))
               .TransitionTo(WaitingToRetry));
            
        
        WhenEnter(FinishedOrder, x => x.Then(
            context => LogContext.Info?.Log("Order replyied: {0}", 
            context.Saga.CorrelationId))
        );

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