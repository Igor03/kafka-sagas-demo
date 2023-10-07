using Contracts;
using Contracts.Configuration;
using MassTransit;

namespace OrdersManagementApi.Extensions;

internal static class KafkaExtensions
{
    internal static IServiceCollection AddCustomKafka(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var kafkaOptions = configuration
            .GetSection("KafkaOptions")
            .Get<KafkaOptions>();
        
        services.AddMassTransit(configureMassTransit =>
        {
            configureMassTransit.UsingInMemory();
            configureMassTransit.AddRider(configureRider =>
            {
                configureRider.AddProducer<string, OrderRequestEvent>(kafkaOptions.Topics.OrderManagementSystemRequest);
                configureRider.UsingKafka(kafkaOptions.ClientConfig, (_, _) => { });
            });
        });
        
        return services;
    }
}