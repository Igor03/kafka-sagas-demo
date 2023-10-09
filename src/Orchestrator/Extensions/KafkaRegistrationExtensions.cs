using Confluent.Kafka;
using MassTransit;
using Orchestrator.Configuration;
using Orchestrator.Consumers;
using Orchestrator.Contracts;

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
                rider.AddProducer<string, SourceSystemResponse>(kafkaTopics.SourceSystemResponse);
                rider.AddProducer<string, ConsumerXRequest>(kafkaTopics.ConsumerXRequest);
                
                rider.AddConsumersFromNamespaceContaining<SourceSystemRequestConsumer>();

                // Setting up two consumers based on data type
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, SourceSystemRequest>(
                       topicName: kafkaTopics.SourceSystemRequest,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.ConfigureConsumer<SourceSystemRequestConsumer>(riderContext);

                           topicConfig.DiscardSkippedMessages();
                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });

                    kafkaConfig.TopicEndpoint<string, ConsumerXResponse>(
                       topicName: kafkaTopics.ConsumerXResponse,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.ConfigureConsumer<ConsumerXResponseConsumer>(riderContext);

                           topicConfig.DiscardSkippedMessages();
                           // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                       });
                });
            });
        });
        
        return services;
    }
}