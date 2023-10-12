using Confluent.Kafka;
using MassTransit;
using Producer.Configuration;
using Producer.Controllers;

namespace Producer.Extensions;

internal static class KafkaExtensions
{
    internal static IServiceCollection AddCustomKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaTopics = configuration.GetSection("KafkaOptions:Topics").Get<KafkaTopics>();
        var clientConfig = configuration.GetSection("KafkaOptions:ClientConfig").Get<ClientConfig>();

        clientConfig.SecurityProtocol = SecurityProtocol.SaslSsl;

        services.AddMassTransit(configureMassTransit =>
        {
            configureMassTransit.UsingInMemory();
            configureMassTransit.AddRider(configureRider =>
            {
                configureRider.AddProducer<string, OrderRequest>(kafkaTopics.SourceSystem);
                configureRider.UsingKafka(clientConfig, (_, _) => { });
            });
        });
        
        return services;
    }
}