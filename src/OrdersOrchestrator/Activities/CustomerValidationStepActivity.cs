using MassTransit;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.Services;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities;

public sealed class CustomerValidationStepActivity : IStateMachineActivity<OrderRequestSagaInstance, CustomerValidationResponseEvent>
{
    private readonly ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer;
    private readonly IApiService apiService;
    
    public CustomerValidationStepActivity(ITopicProducer<TaxesCalculationRequestEvent> taxesCalculationEngineProducer, IApiService apiService)
    {
        this.taxesCalculationEngineProducer = taxesCalculationEngineProducer;
        this.apiService = apiService;
    }
    
    public async Task Execute(
        BehaviorContext<OrderRequestSagaInstance, CustomerValidationResponseEvent> context, 
        IBehavior<OrderRequestSagaInstance, CustomerValidationResponseEvent> next)
    {
        if (await apiService.ValidateIncomingCustomerValidationResult(context.Message))
        {
            throw new Exception("Error during the customer validation!. Not a transient exception");
        }
        
        var taxesCalculationRequestEvent = new TaxesCalculationRequestEvent
        {
            CorrelationId = context.Saga.CorrelationId,
            ItemId = context.Saga.ItemId!,
            CustomerType = context.Message.CustomerType
        };
        
        await taxesCalculationEngineProducer.Produce(taxesCalculationRequestEvent);
        await next.Execute(context);
    }

    public async Task Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, CustomerValidationResponseEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, CustomerValidationResponseEvent> next) 
        where TException : Exception => await next.Execute(context).ConfigureAwait(false);

    public void Probe(ProbeContext context) => context.CreateScope("customer-validation");
    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
}

