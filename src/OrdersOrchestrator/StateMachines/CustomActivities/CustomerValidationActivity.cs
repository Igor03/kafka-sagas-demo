using Contracts;
using Contracts.Exceptions;
using MassTransit;
using OrdersOrchestrator.Services;

namespace OrdersOrchestrator.StateMachines.CustomActivities;

public sealed class CustomerValidationActivity 
    : IStateMachineActivity<OrderRequestSagaInstance, CustomerValidationResponseEvent>
{
    private readonly ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer;
    private readonly IApiService apiService;
    
    public CustomerValidationActivity(
        ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer, 
        IApiService apiService)
    {
        this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
        this.apiService = apiService;
    }
    
    async Task IStateMachineActivity<OrderRequestSagaInstance, CustomerValidationResponseEvent>.Execute(
        BehaviorContext<OrderRequestSagaInstance, CustomerValidationResponseEvent> context, 
        IBehavior<OrderRequestSagaInstance, CustomerValidationResponseEvent> next)
    {
        if (await apiService.ValidateIncomingCustomerValidationResult(context.Message))
        {
            throw new NotATransientException("Error during the customer validation!");
        }
        
        var taxesCalculationRequestEvent = new TaxesCalculationRequestEvent
        {
            ItemId = context.Saga.ItemId!,
            CustomerType = context.Message.CustomerType
        };
        
        await taxesCalculationEngineProducer.Produce(taxesCalculationRequestEvent);
        await next.Execute(context);
    }

    async Task IStateMachineActivity<OrderRequestSagaInstance, CustomerValidationResponseEvent>.Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, CustomerValidationResponseEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, CustomerValidationResponseEvent> next) 
            => await next.Faulted(context).ConfigureAwait(false);

    void IProbeSite.Probe(ProbeContext context) => context.CreateScope(nameof(CustomerValidationActivity));
    void IVisitable.Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}

