using CustomerValidationEngine.Contracts;
using MassTransit;

namespace CustomerValidationEngine.Consumers;

public class CustomerValidationConsumer : IConsumer<CustomerValidationRequest>
{
    private readonly ITopicProducer<string, CustomerValidationResponse> producer;

    public CustomerValidationConsumer(ITopicProducer<string, CustomerValidationResponse> producer)
    {
        this.producer = producer;
    }

    public async Task Consume(ConsumeContext<CustomerValidationRequest> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var response = new CustomerValidationResponse
        {
            CustomerType = GenerateCustomerType(),
            CorrelationId = context.Message.CorrelationId,
        };

        await producer
            .Produce(context.GetKey<string>(), response)
            .ConfigureAwait(false);
    }

    private static string GenerateCustomerType()
    {
        return new Random().Next(1, 5) switch
        {
            1 => "Regular",
            2 => "Premium",
            3 => "Super Premium",
            _ => "Unknown"
        };
    }
}