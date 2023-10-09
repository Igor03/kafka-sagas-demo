using Confluent.Kafka;
using MassTransit;
using Orchestrator.Configuration;
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
                rider.AddProducer<string, SourceSystemResponse>(kafkaTopics.SourceSystemRequest);
                rider.AddProducer<string, ConsumerXRequest>(kafkaTopics.ConsumerXRequest);
                
                // rider.AddConsumersFromNamespaceContaining<SourceSystemRequestConsumer>();
                // rider.AddConsumersFromNamespaceContaining<EngineResponseConsumer>();
                
                // Setting up two consumers based on data type
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                });
            });
        });
        
        return services;
    }
}