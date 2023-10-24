using Confluent.Kafka;
using MassTransit;
using OrdersOrchestrator.Configuration;
using OrdersOrchestrator.Consumers;
using OrdersOrchestrator.Contracts;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;
using OrdersOrchestrator.Middlewares;
using OrdersOrchestrator.StateMachines;

namespace OrdersOrchestrator.Extensions;

public static class KafkaRegistrationExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaTopics = configuration.GetSection("KafkaOptions:Topics").Get<KafkaTopics>();
        var clientConfig = configuration.GetSection("KafkaOptions:ClientConfig").Get<ClientConfig>();
        clientConfig.SecurityProtocol = SecurityProtocol.SaslSsl;

        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
            
            massTransit.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderRequestStateMachine, OrderRequestState>(c =>
                {
                    c.UseMessageRetry(r =>
                    {
                        r.Interval(5, TimeSpan.FromSeconds(1));
                    });
                })
                .InMemoryRepository();
                
                rider.AddProducer<OrderResponseEvent>(kafkaTopics.OrderManagementSystemResponse);
                rider.AddProducer<TaxesCalculationRequestEvent>(kafkaTopics.TaxesCalculationEngineRequest);
                rider.AddProducer<CustomerValidationRequestEvent>(kafkaTopics.CustomerValidationEngineRequest);
                rider.AddProducer<DeadLetterMessage>(kafkaTopics.Deadletter);
                rider.AddProducer<OrderRequestEvent>(kafkaTopics.OrderManagementSystemRequest);
                
                rider.AddConsumersFromNamespaceContaining<OrderManagementSystemConsumer>();

                // Setting up two consumers based on data type
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, OrderRequestEvent>(
                       topicName: kafkaTopics.OrderManagementSystemRequest,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.UseConsumeFilter(typeof(LoggingMiddlewareFilter<>), riderContext);

                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           // topicConfig.ConfigureConsumer<OrderManagementSystemConsumer>(riderContext);
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestState>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);
                           
                           //topicConfig.UseMessageRetry(x =>
                           //{
                           //    x.Handle<ArgumentException>();
                           //    x.Interval(5, TimeSpan.FromSeconds(1));
                           //});
                       });

                    kafkaConfig.TopicEndpoint<string, TaxesCalculationResponseEvent>(
                       topicName: kafkaTopics.TaxesCalculationEngineResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           // topicConfig.ConfigureConsumer<TaxesCalculationEngineConsumer>(riderContext);
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestState>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);

                           //topicConfig.UseMessageRetry(x =>
                           //{
                           //    x.Handle<ArgumentException>();
                           //    x.Interval(5, TimeSpan.FromSeconds(1));
                           //});

                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });

                    kafkaConfig.TopicEndpoint<string, CustomerValidationResponseEvent>(
                       topicName: kafkaTopics.CustomerValidationEngineResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           // topicConfig.ConfigureConsumer<CustomerValidationEngineConsumer>(riderContext);
                           topicConfig.DiscardSkippedMessages();
                           topicConfig.ConfigureSaga<OrderRequestState>(riderContext);
                           topicConfig.UseInMemoryOutbox(riderContext);

                           //topicConfig.UseMessageRetry(x =>
                           //{
                           //    x.Handle<ArgumentException>();
                           //    x.Interval(5, TimeSpan.FromSeconds(1));
                           //});

                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });

                    kafkaConfig.TopicEndpoint<string, DeadLetterMessage>(
                      topicName: kafkaTopics.Deadletter,
                      groupId: kafkaTopics.DefaultGroup,
                      configure: topicConfig =>
                      {
                          topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                          // topicConfig.ConfigureConsumer<DeadLetterConsumer>(riderContext);
                          topicConfig.DiscardSkippedMessages();
                          topicConfig.ConfigureSaga<OrderRequestState>(riderContext);
                      });

                });
            });
        });
        
        return services;
    }
}