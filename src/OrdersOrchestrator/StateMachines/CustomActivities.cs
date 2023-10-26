using MassTransit;
using MassTransit.KafkaIntegration.Activities;
using MassTransit.SagaStateMachine;
using Newtonsoft.Json;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.OrderManagement;

namespace OrdersOrchestrator.StateMachines
{
    public static class CustomActivities
    {
        public static EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> InitializeSaga(
            this EventActivityBinder<OrderRequestSagaInstance, OrderRequestEvent> binder)
        {
            return binder.Then(context =>
            {
                context.Saga.CreatedAt = DateTime.Now;
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.CustomerId = context.Message.CustomerId;
            });
        }

        public static EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> ProcessRetry(
           this EventActivityBinder<OrderRequestSagaInstance, ErrorMessageEvent> binder)
        {
            var @event = binder.Produce(context =>
            {
                context.Saga.UpdatedAt = DateTime.Now;

                var @event = new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CustomerId = context.Saga.CustomerId,
                    ItemId = context.Saga.ItemId,
                    __Header_Registration_RetryAttempt = 2,
                };

                return context.Init<OrderRequestEvent>(@event);
            });

            return @event;
        }

        public static ExceptionActivityBinder<TInstance, TData, TException> SendToErrorTopic<TInstance, TData, TException, TMessage>(
             this ExceptionActivityBinder<TInstance, TData, TException> source,
             Action<SendContext<TMessage>> contextCallback = null)
             where TInstance : class, SagaStateMachineInstance
             where TData : class
             where TMessage : class
             where TException : Exception
        {
            Func<BehaviorExceptionContext<TInstance, TData, TException>, Task<SendTuple<TMessage>>> messageFactory = context =>
            {
                var @event = new
                {
                    context.Saga.CorrelationId,
                    context.Message,
                    __Header_Reason = context.Exception.Message,
                };

                return context.Init<TMessage>(@event);
            };

            return source.Add(new FaultedProduceActivity<TInstance, TData, TException, TMessage>(
                MessageFactory<TMessage>.Create(messageFactory, contextCallback)));
        }

        public static ExceptionActivityBinder<TInstance, TData, TException> Vlah<TInstance, TData, TException, TMessage>(
           this ExceptionActivityBinder<TInstance, TData, TException> source,
           Func<BehaviorExceptionContext<TInstance, TData, TException>, Task<SendTuple<TMessage>>> messageFactory,
           Action<SendContext<TMessage>> contextCallback = null)
           where TInstance : class, SagaStateMachineInstance
           where TData : class
           where TMessage : class
           where TException : Exception
        {
            return source.Add(new FaultedProduceActivity<TInstance, TData, TException, TMessage>(
                MessageFactory<TMessage>.Create(messageFactory, contextCallback)));
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
