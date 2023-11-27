using Confluent.Kafka;
using StackExchange.Redis;

namespace Contracts.Configuration;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
    public MongoDb MongoDb { get; set; } = default!;
    public RedisDb RedisDb { get; set; } = default!;
};

public sealed record Topics
{
    public string OrderManagementSystemRequest { get; set; } = default!;
    public string OrderManagementSystemResponse { get; set; } = default!;
    public string TaxesCalculationEngineRequest { get; set; } = default!;
    public string TaxesCalculationEngineResponse { get; set; } = default!;
    public string CustomerValidationEngineRequest { get; set; } = default!;
    public string CustomerValidationEngineResponse { get; set; } = default!;
    public string Error { get; set; } = default!;
}

public sealed record MongoDb
{
    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
    public string CollectionName { get; set; } = default!;
}

public sealed record RedisDb
{
    public string Endpoint { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string KeyPrefix { get; set; } = default!;
}