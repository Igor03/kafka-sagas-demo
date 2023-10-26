using OrdersOrchestrator.Contracts.ApiService;
using OrdersOrchestrator.Contracts.OrderManagement;

namespace OrdersOrchestrator.Services
{
    public sealed class ApiService : IApiService
    {
        public Task<ApiServiceResponse> SomeApiCallAsync()
        {
            return Task.FromResult(
                new ApiServiceResponse
                {
                    SomeResponseValue = "Some data",
                });
        }

        public Task<bool> ValidateIncomingRequestAsync(OrderRequestEvent @event)
        {
            return Task.FromResult(@event.ItemId!.ToUpper() == "ERROR" || @event.CustomerId!.ToUpper() == "ERROR");
                
        }
    }
}
