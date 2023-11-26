using Contracts;
using MassTransit;
using MassTransit.SagaStateMachine;

namespace OrdersOrchestrator.StateMachines;

public static class OrderRequestStateMachineSupportExtensions
{
    public static EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> InitializeSaga(
        this EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Order requested: {0}", context.CorrelationId);
                
                context.Saga.CreatedAt = DateTime.Now;
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.CustomerId = context.Message.CustomerId;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance, CustomerValidationResponseEvent> UpdateSaga(
        this EventActivityBinder<OrderRequestSagaInstance, CustomerValidationResponseEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Customer validation: {0}", context.CorrelationId);
                
                context.Saga.CustomerType = context.Message.CustomerType;
                context.Saga.UpdatedAt = DateTime.Now;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance, TaxesCalculationResponseEvent> UpdateSaga(
        this EventActivityBinder<OrderRequestSagaInstance, TaxesCalculationResponseEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Taxes calculation: {0}", context.CorrelationId);
                
                context.Saga.UpdatedAt = DateTime.Now;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance> NotifySourceSystem(
        this EventActivityBinder<OrderRequestSagaInstance> binder)
    {
        Func<BehaviorContext<OrderRequestSagaInstance>, Task> asyncAction = async context =>
        {
            LogContext.Info?.Log("Notifying source system: {0}", context.CorrelationId);
            
            await context.GetServiceOrCreateInstance<ITopicProducer<string, NotificationReply<OrderResponseEvent>>>()
                .Produce(
                    Guid.NewGuid().ToString(),
                    context.Saga.NotificationReply!,
                    context.CancellationToken)
                .ConfigureAwait(false);
        };
        
        return binder.Add(new AsyncActivity<OrderRequestSagaInstance>(asyncAction));
    }
}