using Confluent.Kafka;
using MassTransit;
using OrdersOrchestrator.Configuration;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.StateMachines;
using StackExchange.Redis;

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

                rider.AddProducer<ResponseWrapper<OrderResponseEvent>>(kafkaTopics.OrderManagementSystemResponse);
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