using Confluent.Kafka;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrdersOrchestrator.Configuration;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.Database;
using OrdersOrchestrator.Middlewares;
using OrdersOrchestrator.StateMachines;
using System.Reflection;

namespace OrdersOrchestrator.Extensions;

public static class KafkaRegistrationExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaTopics = configuration.GetSection("KafkaOptions:Topics").Get<KafkaTopics>();
        var clientConfig = configuration.GetSection("KafkaOptions:ClientConfig").Get<ClientConfig>();
        // clientConfig.SecurityProtocol = SecurityProtocol.SaslSsl;

        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
            
            massTransit.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderRequestStateMachine, OrderRequestSagaInstance>(c =>
                {
                    c.UseMessageRetry(r => { r.Interval(kafkaTopics.MaxRetriesAttempts, TimeSpan.FromSeconds(3)); });
                }).InMemoryRepository();
                
                //.EntityFrameworkRepository(r =>
                //{
                //    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                //    r.AddDbContext<DbContext, StateMachineDbContext>((provider, builder) =>
                //    {
                //        r.ConcurrencyMode =
                //               ConcurrencyMode.Pessimistic;

                //        r.LockStatementProvider = new PostgresLockStatementProvider();

                //        builder.UseNpgsql("Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=postgres;", m =>
                //        {
                //            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                //            m.MigrationsHistoryTable($"__{nameof(StateMachineDbContext)}");
                //        });
                //    });
                //});
                
                rider.AddProducer<OrderResponseEvent>(kafkaTopics.OrderManagementSystemResponse);
                rider.AddProducer<TaxesCalculationRequestEvent>(kafkaTopics.TaxesCalculationEngineRequest);
                rider.AddProducer<CustomerValidationRequestEvent>(kafkaTopics.CustomerValidationEngineRequest);
                rider.AddProducer<ErrorMessageEvent>(kafkaTopics.Deadletter);
                rider.AddProducer<OrderRequestEvent>(kafkaTopics.OrderManagementSystemRequest);
                
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, OrderRequestEvent>(
                       topicName: kafkaTopics.OrderManagementSystemRequest,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);

                           topicConfig.UseConsumeFilter(typeof(LoggingMiddlewareFilter<>), riderContext);
                           topicConfig.ConfigureError(callback => { callback.UseFilter(new FaultCompensationMiddlewareFilter()); });
                       });

                    kafkaConfig.TopicEndpoint<string, TaxesCalculationResponseEvent>(
                       topicName: kafkaTopics.TaxesCalculationEngineResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                       });

                    kafkaConfig.TopicEndpoint<string, CustomerValidationResponseEvent>(
                       topicName: kafkaTopics.CustomerValidationEngineResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                       });

                    kafkaConfig.TopicEndpoint<string, ErrorMessageEvent>(
                      topicName: kafkaTopics.Deadletter,
                      groupId: kafkaTopics.DefaultGroup,
                      configure: topicConfig =>
                      {
                          topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                          topicConfig.DiscardSkippedMessages();
                          topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);                           
                          topicConfig.UseInMemoryOutbox(riderContext);

                      });

                });
            });
        });
        
        return services;
    }
}