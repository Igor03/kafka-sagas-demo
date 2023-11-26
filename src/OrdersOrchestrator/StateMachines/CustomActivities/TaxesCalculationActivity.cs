using Contracts;
using Contracts.Exceptions;
using MassTransit;
using OrdersOrchestrator.Services;

namespace  OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class TaxesCalculationActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>
{
    private readonly IApiService apiService;
    
    public TaxesCalculationActivity( 
        IApiService apiService)
    {
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

        context.Saga.NotificationReply = new NotificationReply<OrderResponseEvent>
        {
            Success = true,
            Data = new OrderResponseEvent
            {
                CustomerId = context.Saga.CustomerId!,
                CustomerType = context.Saga.CustomerType!,
                TaxesCalculation = context.Message,
            }
        };

        await next
            .Execute(context)
            .ConfigureAwait(false);
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next) 
            => await next.Faulted(context).ConfigureAwait(false);

    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(ReceiveOrderRequestActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}