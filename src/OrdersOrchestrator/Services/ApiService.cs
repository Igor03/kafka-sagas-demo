using MassTransit;
using OrdersOrchestrator.Contracts.ApiService;
using OrdersOrchestrator.Contracts.CustomerValidationEngine;
using OrdersOrchestrator.Contracts.OrderManagement;
using OrdersOrchestrator.Contracts.TaxesCalculationEngine;

namespace OrdersOrchestrator.Services
{
    public sealed class ApiService : IApiService
    {
        public Task<bool> ValidateIncomingOrderRequestAsync(OrderRequestEvent @event)
             => Task.FromResult(@event.CustomerId!.ToUpper() == "ERROR");
        

        public Task<bool> ValidateIncomingCustomerValidationResult(CustomerValidationResponseEvent @event)
            => Task.FromResult(@event.CustomerType == "Unknown");
        

        public Task<bool> ValidateIncomingTaxesCalculationResult(TaxesCalculationResponseEvent @event)
            => Task.FromResult(@event.ItemId!.ToUpper() == "ERROR");
    }
}
