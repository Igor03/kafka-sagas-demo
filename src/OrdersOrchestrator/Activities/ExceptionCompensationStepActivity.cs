using MassTransit;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Activities
{
    public class ExceptionCompensationStepActivity<T> : IStateMachineActivity<OrderRequestState, T>
        where T : class
    {
        public void Accept(StateMachineVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public Task Execute(BehaviorContext<OrderRequestState, T> context, IBehavior<OrderRequestState, T> next)
        {
            throw new NotImplementedException();
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderRequestState, T, TException> context, IBehavior<OrderRequestState, T> next) where TException : Exception
        {
            throw new NotImplementedException();
        }

        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }
    }
}
