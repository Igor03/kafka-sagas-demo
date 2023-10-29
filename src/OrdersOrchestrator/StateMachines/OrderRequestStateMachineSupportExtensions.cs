using MassTransit;
using MassTransit.KafkaIntegration.Activities;
using MassTransit.SagaStateMachine;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines;

public static class OrderRequestStateMachineSupportExtensions
{
    public static EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> InitializeSaga(
        this EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Order requested: {0}", context.Saga.CorrelationId);
                context.Saga.CreatedAt = DateTime.Now;
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.CustomerId = context.Message.CustomerId;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance, CustomerValidationResponseEvent> UpdateSaga(
        this EventActivityBinder<OrderRequestSagaInstance, CustomerValidationResponseEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Customer validation: {0}", context.Saga.CorrelationId);
                context.Saga.CustomerType = context.Message.CustomerType;
                context.Saga.UpdatedAt = DateTime.Now;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance, TaxesCalculationResponseEvent> UpdateSaga(
        this EventActivityBinder<OrderRequestSagaInstance, TaxesCalculationResponseEvent> binder)
        => binder.Then(context =>
            {
                LogContext.Info?.Log("Taxes calculation: {0}", context.Saga.CorrelationId);
                context.Saga.UpdatedAt = DateTime.Now;
            });
    
    public static EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> NotifySourceSystem(
       this EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> binder)
    {
        var @event = binder.Produce(context =>
        {
            context.Saga.UpdatedAt = DateTime.Now;

            var @event = new
            {
                Success = false,
                context.Saga.Reason,
                __Header_Reason = context.Saga.Reason,
            };

            return context.Init<ResponseWrapper<OrderResponseEvent>>(@event);
        });

        return @event;
    }
    
    public static EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> RestartProcess(
        this EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> binder)
    {
        var @event = binder.Produce(context =>
        {
            context.Saga.UpdatedAt = DateTime.Now;
            context.Saga.Attempts++;

            var @event = new
            {
                 context.Saga.CorrelationId,
                 context.Saga.CustomerId,
                 context.Saga.ItemId,
                __Header_Attempt = context.Saga.Attempts
            };

            return context.Init<OrderRequestEvent>(@event);
        });

        return @event;
    }

    public static ExceptionActivityBinder<TInstance, TData, TException> SendErrorEvent<TInstance, TData, TException>(
         this ExceptionActivityBinder<TInstance, TData, TException> source,
         Action<SendContext<ErrorMessageEvent>> contextCallback = default!)
         where TInstance : OrderRequestSagaInstance
         where TData : class
         where TException : Exception
    {
        Func<BehaviorExceptionContext<TInstance, TData, TException>, Task<SendTuple<ErrorMessageEvent>>> messageFactory = context =>
        {
            // Updating Saga with exception message
            context.Saga.Reason = context.Exception.Message;
            context.Saga.UpdatedAt = DateTime.Now;
            
            LogContext.Info?.Log("Processing fault for {0}: {1}", context.Event.Name, context.Saga.CorrelationId);
            
            var @event = new
            {
                context.Saga.CorrelationId,
                context.Message,
                __Header_Reason = context.Exception.Message,
                __Header_Attempts = context.Saga.Attempts
            };

            return context.Init<ErrorMessageEvent>(@event);
        };

        return source.Add(new FaultedProduceActivity<TInstance, TData, TException, ErrorMessageEvent>(
            MessageFactory<ErrorMessageEvent>.Create(messageFactory, contextCallback)));
    }
}
