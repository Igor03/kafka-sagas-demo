using MassTransit;
using TaxesCalculationEngine.Contracts;

namespace TaxexCalculationEngine.Consumers
{
    public class TaxesCalculationConsumer : IConsumer<TaxesCalculationRequest>
    {
        private readonly ITopicProducer<string, TaxesCalculationResponse> taxesCalculationProducer;
   
        public TaxesCalculationConsumer(ITopicProducer<string, TaxesCalculationResponse> taxesCalculationProducer)
        {
            this.taxesCalculationProducer = taxesCalculationProducer;
        }

        public async Task Consume(ConsumeContext<TaxesCalculationRequest> context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var rnd = new Random();
            var response = new TaxesCalculationResponse
            {
                ItemId = context.Message.ItemId,
                TaxAAA = (decimal)rnd.NextInt64(0, 100) / 10,
                TaxBBB = (decimal)rnd.NextInt64(0, 100) / 10,
                TaxCCC = (decimal)rnd.NextInt64(0, 100) / 10,
            };

            response.CorrelationId = context.Message.CorrelationId;
            
            // Sending the calculated taxes based on the ItemId
            await taxesCalculationProducer
                .Produce(context.GetKey<string>(), response)
                .ConfigureAwait(false);
        }
    }
}
