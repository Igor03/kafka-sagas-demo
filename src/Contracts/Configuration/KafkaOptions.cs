using Confluent.Kafka;

namespace Contracts.Configuration;

public sealed record KafkaOptions
{
    public string ConsumerGroup { get; set; } = default!;
    public Topics Topics { get; set; } = default!;
    public ClientConfig ClientConfig { get; set; } = default!;
    public MongoDb MongoDb { get; set; } = default!;
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
    public string Database { get; set; } = default!;
}