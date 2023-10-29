using MassTransit;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.Services;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities;

public sealed class TaxesCalculationStepActivity : IStateMachineActivity<OrderRequestSagaInstance, TaxesCalculationResponseEvent>
{
    private readonly ITopicProducer<ResponseWrapper<OrderResponseEvent>> orderResponseEventProducer;
    private readonly IApiService apiService;
    
    public TaxesCalculationStepActivity(
        ITopicProducer<ResponseWrapper<OrderResponseEvent>> orderResponseEventProducer, 
        IApiService apiService)
    {
        this.orderResponseEventProducer = orderResponseEventProducer;
        this.apiService = apiService;
    }

    public async Task Execute(
        BehaviorContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent> context, 
        IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next)
    {
        if (await apiService.ValidateIncomingTaxesCalculationResult(context.Message))
        {
            throw new Exception("Error during the taxes calculation. Not a transient exception");
        }

        var orderResponseEvent = new OrderResponseEvent
        {
            CorrelationId = context.Saga.CorrelationId,
            CustomerId = context.Saga.CustomerId,
            CustomerType= context.Saga.CustomerType,
            TaxesCalculation = context.Message,
            FinishedAt = DateTime.Now,
        };

        await orderResponseEventProducer.Produce(new ResponseWrapper<OrderResponseEvent>
        {
            Data = orderResponseEvent,
            Success = true,
        });

        await next.Execute(context);
    }

    public async Task Faulted<TException>(
        BehaviorExceptionContext<OrderRequestSagaInstance, TaxesCalculationResponseEvent, TException> context, 
        IBehavior<OrderRequestSagaInstance, TaxesCalculationResponseEvent> next) 
        where TException : Exception => await next.Execute(context).ConfigureAwait(false);

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);
    public void Probe(ProbeContext context) => context.CreateScope("taxes-calculation");
}