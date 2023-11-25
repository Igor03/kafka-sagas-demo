using Confluent.Kafka;
using Contracts;
using Contracts.Configuration;
using MassTransit;
using OrdersOrchestrator.StateMachines;
using StackExchange.Redis;

namespace OrdersOrchestrator.Extensions;

public static class KafkaRegistrationExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration
            .GetSection(nameof(KafkaOptions))
            .Get<KafkaOptions>();
        // clientConfig.SecurityProtocol = SecurityProtocol.SaslSsl;

        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) =>  cfg.ConfigureEndpoints(context));
            massTransit.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderRequestStateMachine, OrderRequestSagaInstance>(typeof(OrderRequestSagaDefinition))
                // .InMemoryRepository();
                .RedisRepository(p =>
                {
                    var redisOptions = new ConfigurationOptions
                    {
                        EndPoints = { "127.0.0.1:6379" },
                        Password = "eYVX7EwVmmxKPCDmwMtyKVge8oLd2t81",
                    };
                    
                    // Since we have multiple services and states
                    p.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    p.DatabaseConfiguration(redisOptions);
                });

                rider.AddProducer<NotificationReply<OrderResponseEvent>>(kafkaOptions.Topics.OrderManagementSystemResponse);
                rider.AddProducer<TaxesCalculationRequestEvent>(kafkaOptions.Topics.TaxesCalculationEngineRequest);
                rider.AddProducer<CustomerValidationRequestEvent>(kafkaOptions.Topics.CustomerValidationEngineRequest);
                rider.AddProducer<FaultMessageEvent>(kafkaOptions.Topics.Error);
                rider.AddProducer<OrderRequestEvent>(kafkaOptions.Topics.OrderManagementSystemRequest);
                
                rider.UsingKafka(kafkaOptions.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, OrderRequestEvent>(
                       topicName: kafkaOptions.Topics.OrderManagementSystemRequest,
                       groupId: kafkaOptions.ConsumerGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                       });

                    kafkaConfig.TopicEndpoint<string, TaxesCalculationResponseEvent>(
                       topicName: kafkaOptions.Topics.TaxesCalculationEngineResponse,
                       groupId: kafkaOptions.ConsumerGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                       });

                    kafkaConfig.TopicEndpoint<string, CustomerValidationResponseEvent>(
                       topicName: kafkaOptions.Topics.CustomerValidationEngineResponse,
                       groupId: kafkaOptions.ConsumerGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestSagaInstance>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                       });

                    kafkaConfig.TopicEndpoint<string, FaultMessageEvent>(
                      topicName: kafkaOptions.Topics.Error,
                      groupId: kafkaOptions.Topics.Error,
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