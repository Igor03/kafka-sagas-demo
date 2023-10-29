using OrdersOrchestrator.Contracts.ApiService;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Services
{
    public interface IApiService
    {
        Task<bool> ValidateIncomingOrderRequestAsync(OrderRequestEvent @event);
        Task<bool> ValidateIncomingCustomerValidationResult(CustomerValidationResponseEvent @event);
        Task<bool> ValidateIncomingTaxesCalculationResult(TaxesCalculationResponseEvent @event);
    }
}
