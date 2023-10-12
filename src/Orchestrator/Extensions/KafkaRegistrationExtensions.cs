using Confluent.Kafka;
using MassTransit;
using Orchestrator.Configuration;
using Orchestrator.Consumers;
using Orchestrator.Contracts.OrderManagement;
using Orchestrator.Contracts.TaxesCalculationEngine;

namespace Orchestrator.Extensions;

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
                rider.AddProducer<string, OrderResponse>(kafkaTopics.OrderManagementSystemResponse);
                rider.AddProducer<string, TaxesCalculationRequest>(kafkaTopics.TaxesCalculationEngineRequest);
                
                rider.AddConsumersFromNamespaceContaining<OrderManagementSystemConsumer>();

                // Setting up two consumers based on data type
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, OrderRequest>(
                       topicName: kafkaTopics.OrderManagementSystemRequest,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.ConfigureConsumer<OrderManagementSystemConsumer>(riderContext);

                           topicConfig.DiscardSkippedMessages();
                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });

                    kafkaConfig.TopicEndpoint<string, TaxesCalculationResponse>(
                       topicName: kafkaTopics.TaxesCalculationEngineResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.ConfigureConsumer<TaxesCalculationEngineConsumer>(riderContext);

                           topicConfig.DiscardSkippedMessages();
                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });
                });
            });
        });
        
        return services;
    }
}