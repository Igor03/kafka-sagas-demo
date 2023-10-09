using Confluent.Kafka;
using ConsumerX.Configuration;
using ConsumerX.Consumers;
using ConsumerX.Contracts;
using MassTransit;

namespace ConsumerX.Extensions
{
    public static class KafkaRgistrationExtensions
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
                    rider.AddProducer<string, ConsumerXResponse>(kafkaTopics.ConsumerXResponse);
                    rider.AddConsumersFromNamespaceContaining<XRequestConsumer>();

                    // Setting up two consumers based on data type
                    rider.UsingKafka(clientConfig, (riderContext, kafkaConfig) =>
                    {
                        kafkaConfig.TopicEndpoint<string, ConsumerXRequest>(
                           topicName: kafkaTopics.ConsumerXRequest,
                           groupId: kafkaTopics.DefaultGroup,
                           configure: topicConfig =>
                           {
                               topicConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                               topicConfig.ConfigureConsumer<XRequestConsumer>(riderContext);

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
