using Contracts;
using Contracts.Exceptions;
using MassTransit;
using OrdersOrchestrator.Services;

namespace  OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class TaxesCalculationStepActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>
{
    private readonly ITopicProducer<NotificationReply<OrderResponseEvent>> orderResponseEventProducer;
    private readonly IApiService apiService;
    
    public TaxesCalculationStepActivity(
        ITopicProducer<NotificationReply<OrderResponseEvent>> orderResponseEventProducer, 
        IApiService apiService)
    {
        this.orderResponseEventProducer = orderResponseEventProducer;
        this.apiService = apiService;
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>.Execute(
        BehaviorContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent> context, 
        IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next)
    {
        if (await apiService.ValidateIncomingTaxesCalculationResult(context.Message))
        {
            throw new NotATransientException("Error during order request validation!");
        }

        var orderResponseEvent = new OrderResponseEvent
        {
            CustomerId = context.Saga.CustomerId!,
            CustomerType= context.Saga.CustomerType!,
            TaxesCalculation = context.Message,
        };

        await orderResponseEventProducer.Produce(new NotificationReply<OrderResponseEvent>
        {
            Data = orderResponseEvent,
            Success = true,
        });

        await next.Execute(context);
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next) 
            => await next.Faulted(context).ConfigureAwait(false);

    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(ReceiveOrderRequestStepActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}