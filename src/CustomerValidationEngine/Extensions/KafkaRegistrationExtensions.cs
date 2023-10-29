using Confluent.Kafka;
using CustomerValidationEngine.Configuration;
using CustomerValidationEngine.Consumers;
using CustomerValidationEngine.Contracts;
using MassTransit;

namespace CustomerValidationEngine.Extensions;

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
                rider.AddProducer<string, CustomerValidationResponse>(kafkaTopics.CustomerValidationEngineResponse);
                rider.AddConsumersFromNamespaceContaining<CustomerValidationConsumer>();

                // Receiving Taxes Calculation request
                rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, CustomerValidationRequest>(
                       topicName: kafkaTopics.CustomerValidationEngineRequest,
                       groupId: kafkaTopics.DefaultGroup,
                       configure: topicConfig =>
                       {
                           topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                           topicConfig.ConfigureConsumer<CustomerValidationConsumer>(riderContext);
                           topicConfig.DiscardSkippedMessages();
                       });
                });
            });
        });

        return services;
    }
}

