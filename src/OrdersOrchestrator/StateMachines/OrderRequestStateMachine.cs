using MassTransit;
using MongoDB.Bson;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public class OrderRequestStateMachine : MassTransitStateMachine<OrderRequestState>
{
    public OrderRequestStateMachine()
    {
        InstanceState(x => x.CurrentState, Placed);

        Event(() => OrderRequestedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CustomerValidationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => TaxesCalculationResponseEvent, x => x.CorrelateById(context => context.Message.CorrelationId));


        Initially(
            When(OrderRequestedEvent)
            .Then(x =>
            {
                x.Saga.ItemId = x.Message.ItemId;

                Console.WriteLine("Receiving order request");
                Console.WriteLine(x.Saga.ToJson());
            })
             .TransitionTo(ValidatingCustomer));


        During(ValidatingCustomer,
          When(CustomerValidationResponseEvent)
              .Produce(context =>
              {
                  context.Saga.CustomerId = context.Message.AdjudtedCustomerId;

                  Console.WriteLine("Validating Customer");
                  Console.WriteLine(context.Saga.ToJson());

                  var @event = new TaxesCalculationRequestEvent
                  {
                      CorrelationId = context.Saga.CorrelationId,
                      ItemId = context.Saga.ItemId
                  };

                  return context.Init<TaxesCalculationRequestEvent>(@event);
              })
              .TransitionTo(CalculatingTaxes));

        During(CalculatingTaxes,
            When(TaxesCalculationResponseEvent)
                .Produce(context =>
                {
                    Console.WriteLine("Calculating taxes");
                    Console.WriteLine(context.Saga.ToJson());

                    var @event = new OrderResponseEvent
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        TaxesCalculation = context.Message,
                        FinishedAt = DateTime.Now
                    };

                    return context.Init<OrderResponseEvent>(@event);
                })
                .TransitionTo(FinishedOrder));

       SetCompletedWhenFinalized();
    }

    public State? Placed { get; }

    public State? ValidatingCustomer { get; }

    public State? CalculatingTaxes { get; }
    
    public State? FinishedOrder { get; set; }
    
    public Event<OrderRequestEvent>? OrderRequestedEvent { get; }

    public Event<CustomerValidationResponseEvent>? CustomerValidationResponseEvent { get; }
    
    public Event<TaxesCalculationResponseEvent>? TaxesCalculationResponseEvent { get; }
}