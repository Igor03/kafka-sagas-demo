using Contracts;
using MassTransit;

namespace CustomerValidationEngine.Consumers;

public sealed class CustomerValidationConsumer : IConsumer<CustomerValidationRequestEvent>
{
    private readonly ITopicProducer<string, CustomerValidationResponseEvent> producer;

    public CustomerValidationConsumer(ITopicProducer<string,CustomerValidationResponseEvent> producer)
    {
        this.producer = producer;
    }

    async Task IConsumer<CustomerValidationRequestEvent>
        .Consume(ConsumeContext<CustomerValidationRequestEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var key = Guid.NewGuid();
            
        var response = new CustomerValidationResponseEvent
        {
            CustomerType = GenerateCustomerType(),
        };

        await producer
            .Produce(key.ToString(), response)
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