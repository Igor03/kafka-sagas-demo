using MassTransit;
using MassTransit.SagaStateMachine;
using Newtonsoft.Json;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.StateMachines
{
    public static class CustomActivities
    {
        public static EventActivityBinder<OrderRequestState, OrderRequestEvent> InitializeSaga(
            this EventActivityBinder<OrderRequestState, OrderRequestEvent> binder)
        {
            return binder.Then(context =>
            {
                context.Saga.CreatedAt = DateTime.Now;
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.CustomerId = context.Message.CustomerId;
            });
        }
        
        public static EventActivityBinder<OrderRequestState, CustomerValidationResponseEvent> SendToTaxesCalculation(
            this EventActivityBinder<OrderRequestState, CustomerValidationResponseEvent> binder)
        {
            var @event = binder.Produce(context =>
            {
                // Updating Saga
                context.Saga.CustomerType = context.Message.CustomerType;
                context.Saga.UpdatedAt = DateTime.Now;
                
                var @event = new
                {
                    context.Message.CustomerType,
                    context.Saga.CorrelationId,
                    ItemId = context.Saga.ItemId!,

                };
                return context.Init<TaxesCalculationRequestEvent>(@event);
            });
            return @event;
        }
        
        public static EventActivityBinder<OrderRequestState, TaxesCalculationResponseEvent> ReplyToRequestingSystem(
            this EventActivityBinder<OrderRequestState, TaxesCalculationResponseEvent> binder)
        {
            var @event = binder.Produce(context =>
            {
                context.Saga.UpdatedAt = DateTime.Now;

                var @event = new OrderResponseEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CustomerId = context.Saga.CustomerId,
                    CustomerType = context.Saga.CustomerType,
                    TaxesCalculation = context.Message,
                    FinishedAt = DateTime.Now,
                };

                return context.Init<OrderResponseEvent>(@event);
            });
            
            return @event;
        }

        public static EventActivityBinder<OrderRequestState, DeadLetterMessage> ProcessRetry(
           this EventActivityBinder<OrderRequestState, DeadLetterMessage> binder)
        {
            var @event = binder.Produce(context =>
            {
                context.Saga.UpdatedAt = DateTime.Now;

                var @event = new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CustomerId = context.Saga.CustomerId,
                    ItemId = context.Saga.ItemId,
                    __Header_Registration_RetryAttempt = context.Saga.RetryAttempt++
                };

                return context.Init<OrderRequestEvent>(@event);
            });

            return @event;
        }

        public static EventActivityBinder<TSaga, TData> LogSaga<TSaga, TData>(this EventActivityBinder<TSaga, TData> binder)
            where TSaga : class, ISaga
            where TData : class
        {
            Action<BehaviorContext<TSaga, TData>> logAction = context =>
                LogContext.Info?.Log(JsonConvert.SerializeObject(context.Saga, Formatting.Indented));
            
            return binder.Add(new ActionActivity<TSaga, TData>(logAction));
        }
    }
}
