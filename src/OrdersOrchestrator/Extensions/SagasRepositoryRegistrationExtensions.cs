using Contracts.Configuration;
using MassTransit;
using StackExchange.Redis;

namespace OrdersOrchestrator.Extensions;

internal static class SagasRepositoryRegistrationExtensions
{
    internal static ISagaRegistrationConfigurator<T> ConfigureRedisRepository<TStateMachine, T>(
        this ISagaRegistrationConfigurator<T> configurator,
        RedisDb redisOptions)
        where TStateMachine : class, SagaStateMachine<T>
        where T : class, SagaStateMachineInstance, ISagaVersion
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));   
        ArgumentNullException.ThrowIfNull(redisOptions, nameof(redisOptions));

        return configurator.RedisRepository(configure =>
        {
            var options = new ConfigurationOptions
            {
                EndPoints = { redisOptions.Endpoint },
                Password = redisOptions.Password
            };

            configure.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            configure.KeyPrefix = redisOptions.KeyPrefix;
            configure.DatabaseConfiguration(options);
        });
    }
}