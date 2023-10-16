using CustomerValidationEngine.Contracts;
using MassTransit;

namespace TaxexCalculationEngine.Consumers
{
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
                AdjudtedCustomerId = $"123valid-{context.Message.CustomerId}",
                CorrelationId = context.Message.CorrelationId,
            };
           
            await producer
                .Produce(context.GetKey<string>(), response)
                .ConfigureAwait(false);
        }
    }
}
