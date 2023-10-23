using OrdersOrchestrator.Contracts.ApiService;

namespace OrdersOrchestrator.Services
{
    public interface IApiService
    {
        Task<ApiServiceResponse> SomeApiCallAsync();

        Task<bool> ValidateIncomingRequestAsync();
    }
}
