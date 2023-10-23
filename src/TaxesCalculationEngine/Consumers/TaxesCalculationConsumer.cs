using MassTransit;
using MassTransit.SagaStateMachine;
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

            var customerType = context.Message.CustomerType!;
            
            var calculationFactor = customerType switch
            {
                "Regular" => 100,
                "Premium" => 50,
                "Super Premium" => 20,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var rnd = new Random();
            var response = new TaxesCalculationResponse
            {
                CorrelationId = context.Message.CorrelationId,
                ItemId = context.Message.ItemId!,
                TaxAAA = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
                TaxBBB = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
                TaxCCC = (decimal)rnd.NextInt64(0, calculationFactor) / 10,
            };

            // Sending the calculated taxes based on the ItemId
            await taxesCalculationProducer
                .Produce(context.GetKey<string>(), response)
                .ConfigureAwait(false);
        }
    }
}
