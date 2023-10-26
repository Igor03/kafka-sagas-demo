using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Database
{
    public class OrderRequestSagaInstanceMap : SagaClassMap<OrderRequestSagaInstance>
    {
        protected override void Configure(EntityTypeBuilder<OrderRequestSagaInstance> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasColumnType("varchar");
            entity.Property(x => x.Version).HasColumnType("int");
            entity.Property(x => x.ItemId).HasColumnType("varchar");
            entity.Property(x => x.CustomerType).HasColumnType("varchar");
            entity.Property(x => x.CustomerId).HasColumnType("varchar");
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp");
            entity.Property(x => x.Reason).HasColumnType("varchar");

            base.Configure(entity, model);
        }
    }
}
