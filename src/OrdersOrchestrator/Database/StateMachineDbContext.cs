using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace OrdersOrchestrator.Database;

    public class StateMachineDbContext : SagaDbContext
    {
        public StateMachineDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderRequestSagaInstanceMap(); }
        }
    }
