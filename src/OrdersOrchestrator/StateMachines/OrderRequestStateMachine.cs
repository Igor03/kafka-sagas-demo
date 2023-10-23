using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestState>
{
    public OrderRequestStateMachine()
    {
        InstanceState(x => x.CurrentState, Placed);
        
        // Registering and correlating events
        CorrelateEvents();
        
        Initially(
            When(OrderRequestedEvent)
                .Then(_ => LogContext.Info?.Log("Receiving order request"))
                .InitializeSaga()
                .LogSaga()
                .TransitionTo(ValidatingCustomer));
        
        During(ValidatingCustomer,
          When(CustomerValidationResponseEvent)
              .Then(_ => LogContext.Info?.Log("Validating customer"))
              .SendToTaxesCalculation()
              .LogSaga()
              .TransitionTo(CalculatingTaxes));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Then(_ => LogContext.Info?.Log("Calculating taxes"))
                .ReplyToRequestingSystem()
                .LogSaga()
                .TransitionTo(FinishedOrder));
        
        WhenEnter(FinishedOrder, x => x.Then(_ => LogContext.Info?.Log("Finished")));
        WhenEnter(Initial, x => x.Then(_ => LogContext.Debug?.Log("Received order>>>>>>")));
        
        // Marking as completed for the SM instance to be removed from the repository
       SetCompletedWhenFinalized();
    }

    private void CorrelateEvents()
    {
        Event(() => OrderRequestedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CustomerValidationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => TaxesCalculationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    }

    public State? Placed { get; set; }
    public State? ValidatingCustomer { get; set; }
    public State? CalculatingTaxes { get; set; }
    public State? FinishedOrder { get; set; }
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; set; }
    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; set; }
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; set; }
}