﻿using Contracts;
using MassTransit;
using MassTransit.KafkaIntegration.Activities;
using MassTransit.SagaStateMachine;

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
    
    public static EventActivityBinder<OrderRequestSagaInstance, FaultMessageEvent> NotifySourceSystem(
       this EventActivityBinder<OrderRequestSagaInstance, FaultMessageEvent> binder)
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

            return context.Init<NotificationReply<OrderResponseEvent>>(@event);
        });

        return @event;
    }
    
    public static EventActivityBinder<OrderRequestSagaInstance, FaultMessageEvent> RestartProcess(
        this EventActivityBinder<OrderRequestSagaInstance, FaultMessageEvent> binder)
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
         Action<SendContext<FaultMessageEvent>> contextCallback = default!)
         where TInstance : OrderRequestSagaInstance
         where TData : class
         where TException : Exception
    {
        Func<BehaviorExceptionContext<TInstance, TData, TException>, Task<SendTuple<FaultMessageEvent>>> messageFactory = context =>
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

            return context.Init<FaultMessageEvent>(@event);
        };

        return source.Add(new FaultedProduceActivity<TInstance, TData, TException, FaultMessageEvent>(
            MessageFactory<FaultMessageEvent>.Create(messageFactory, contextCallback)));
    }
}
