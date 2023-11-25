using Confluent.Kafka;
using Contracts;
using Contracts.Configuration;
using MassTransit;
using TaxesCalculationEngine.Consumers;

namespace TaxesCalculationEngine.Extensions;

public static class KafkaRegistrationExtensions
{
    internal static IServiceCollection AddCustomKafka(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration.GetSection("KafkaOptions").Get<KafkaOptions>();
        // clientConfig.SecurityProtocol = SecurityProtocol.SaslSsl;

        services.AddMassTransit(massTransit =>
        {
            massTransit.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });

            massTransit.AddRider(rider =>
            {
                rider.AddProducer<string, TaxesCalculationResponseEvent>(kafkaOptions.Topics.TaxesCalculationEngineResponse);
                rider.AddConsumersFromNamespaceContaining<TaxesCalculationConsumer>();

                rider.UsingKafka(kafkaOptions.ClientConfig, (riderContext, kafkaConfig) =>
                {
                    kafkaConfig.TopicEndpoint<string, TaxesCalculationRequestEvent>(
                        topicName: kafkaOptions.Topics.TaxesCalculationEngineRequest,
                        groupId: kafkaOptions.ConsumerGroup,
                        configure: topicConfig =>
                        {
                            topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                            topicConfig.ConfigureConsumer<TaxesCalculationConsumer>(riderContext);
                            topicConfig.DiscardSkippedMessages();
                        });
                });
            });
        });

        return services;
    }
}