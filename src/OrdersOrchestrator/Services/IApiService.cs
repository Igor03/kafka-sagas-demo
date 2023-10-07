using Contracts;

namespace OrdersOrchestrator.Services
{
    public interface IApiService
    {
        Task<bool> ValidateIncomingOrderRequestAsync(OrderRequestEvent @event);
        Task<bool> ValidateIncomingCustomerValidationResult(CustomerValidationResponseEvent @event);
        Task<bool> ValidateIncomingTaxesCalculationResult(TaxesCalculationResponseEvent @event);
    }
}
