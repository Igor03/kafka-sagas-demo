using Contracts;
using MassTransit;

namespace TaxesCalculationEngine.Consumers;

public sealed class TaxesCalculationConsumer : IConsumer<TaxesCalculationRequestEvent>
{
    private readonly ITopicProducer<string, TaxesCalculationResponseEvent> producer;

    public TaxesCalculationConsumer(ITopicProducer<string, TaxesCalculationResponseEvent> producer)
    {
        this.producer = producer;
    }

    async Task IConsumer<TaxesCalculationRequestEvent>
        .Consume(ConsumeContext<TaxesCalculationRequestEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        
        var key = Guid.NewGuid();
        var calculationFactor = GenerateCalculationFactor(context.Message.CustomerType);
        var response = BuildResponse(context.Message.ItemId, calculationFactor);
        
        // Sending the calculated taxes based on the ItemId and the CustomerType
        await producer
            .Produce(key.ToString(), response)
            .ConfigureAwait(false);
    }
    
    private static int GenerateCalculationFactor(string customerType) 
        // This might be an enum
        => customerType switch
            {
                "Regular" => 100,
                "Premium" => 50,
                "Super Premium" => 20,
                _ => throw new ArgumentOutOfRangeException()
            };

    private static TaxesCalculationResponseEvent BuildResponse(string itemId, int calculationFactor)
    {
        var rnd = new Random();
        
        var response = new TaxesCalculationResponseEvent
        {
            ItemId = itemId,
            TaxAAA = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
            TaxBBB = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
            TaxCCC = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
        };

        return response;
    }
}