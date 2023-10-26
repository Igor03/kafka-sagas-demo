using Confluent.Kafka;
using ConsumerX.Configuration;
using MassTransit;
using TaxesCalculationEngine.Contracts;
using TaxexCalculationEngine.Consumers;

namespace TaxesCalculationEngine.Extensions
{
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
                    rider.AddProducer<string, TaxesCalculationResponse>(kafkaTopics.TaxesCalculationEngineResponse);
                    rider.AddConsumersFromNamespaceContaining<TaxesCalculationConsumer>();

                    // Receiving Taxes Calculation request
                    rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                    {
                        kafkaConfig.TopicEndpoint<string, TaxesCalculationRequest>(
                           topicName: kafkaTopics.TaxesCalculationEngineRequest,
                           groupId: kafkaTopics.DefaultGroup,
                           configure: topicConfig =>
                           {
                               topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                               topicConfig.ConfigureConsumer<TaxesCalculationConsumer>(riderContext);

                               topicConfig.DiscardSkippedMessages();
                               // topicConfig.UseConsumeFilter(typeof(TelemetryInterceptorMiddlewareFilter<>), riderContext);  
                           });
                    });
                });
            });

            return services;
        }
    }
}
