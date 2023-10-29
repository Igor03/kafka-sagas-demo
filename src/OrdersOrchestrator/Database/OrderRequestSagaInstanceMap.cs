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
            model.AddInboxStateEntity();
            model.AddOutboxMessageEntity();
            model.AddOutboxStateEntity();
            
            // Configuring saga table
            entity.ToTable("order_request_state").HasKey(p => p.CorrelationId);
            entity.Property(x => x.CorrelationId).HasColumnType("uuid").HasColumnName("correlation_id");
            entity.Property(x => x.CurrentState).HasColumnType("varchar").HasColumnName("current_state");
            entity.Property(x => x.Version).HasColumnType("int").HasColumnName("version");
            entity.Property(x => x.ItemId).HasColumnType("varchar").HasColumnName("item_id");
            entity.Property(x => x.CustomerType).HasColumnType("varchar").HasColumnName("customer_type");
            entity.Property(x => x.CustomerId).HasColumnType("varchar").HasColumnName("customer_id");
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp").HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp").HasColumnName("updated_at");
            entity.Property(x => x.Reason).HasColumnType("varchar").HasColumnName("reason");
            // entity.Property(x => x.RowVersion).IsRowVersion().HasColumnName("row_version");

            base.Configure(entity, model);
        }
    }
}
