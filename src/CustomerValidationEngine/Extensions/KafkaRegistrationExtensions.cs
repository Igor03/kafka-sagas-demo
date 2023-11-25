using Confluent.Kafka;
using Contracts;
using Contracts.Configuration;
using CustomerValidationEngine.Consumers;
using MassTransit;

namespace CustomerValidationEngine.Extensions;

public static class KafkaRegistrationExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration
            .GetSection("KafkaOptions")
            .Get<KafkaOptions>();
        
        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            massTransit.AddRider(rider =>
            {
                rider.AddProducer<string, CustomerValidationResponseEvent>(kafkaOptions.Topics.CustomerValidationEngineResponse);
                rider.AddConsumersFromNamespaceContaining<CustomerValidationConsumer>();

                rider.UsingKafka(kafkaOptions.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, CustomerValidationRequestEvent>(
                       topicName: kafkaOptions.Topics.CustomerValidationEngineRequest,
                       groupId: kafkaOptions.ConsumerGroup,
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

