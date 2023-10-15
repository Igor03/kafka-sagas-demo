using MassTransit;
using MassTransit.Logging;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestState>
{
    public OrderRequestStateMachine()
    {
        InstanceState(x => x.CurrentState, Placed);

        Event(() => OrderRequestedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => TaxesCalculationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        
        Initially(
            When(OrderRequestedEvent)
                .Produce(context => context.Init<TaxesCalculationRequestEvent>(new TaxesCalculationRequestEvent
                {
                     ItemId = "From my saga", 
                     CorrelationId = context.Saga.CorrelationId
                }))
                .TransitionTo(CalculatingTaxes));
        
        // During(CalculatingTaxes,
        //     When(TaxesCalculationResponseEvent)
        //         .Produce(context => context.Init<OrderResponseEvent>(new OrderResponseEvent
        //         {
        //             CorrelationId = context.Saga.CorrelationId,
        //             TaxesCalculation = context.Message,
        //             FinishedAt = DateTime.Now
        //             
        //         }))
        //         
        //         .TransitionTo(FinishedOrder));
        
        SetCompletedWhenFinalized();
    }
    
    public State Placed { get; }
    
    public State CalculatingTaxes { get; }
    
    public State FinishedOrder { get; set; }
    
    public Event<OrderRequestEvent> OrderRequestedEvent { get; }
    
    public Event<TaxesCalculationResponseEvent> TaxesCalculationResponseEvent { get; }
}