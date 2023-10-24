using OrdersOrchestrator.Contracts.ApiService;
using OrdersOrchestrator.Contracts.OrderManagement;

namespace OrdersOrchestrator.Services
{
    public interface IApiService
    {
        Task<ApiServiceResponse> SomeApiCallAsync();

        Task<bool> ValidateIncomingRequestAsync(OrderRequestEvent @event);
    }
}
