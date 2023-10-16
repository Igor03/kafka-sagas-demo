using OrdersOrchestrator.Contracts.ApiService;

namespace OrdersOrchestrator.Services
{
    public interface IApiService
    {
        Task<ApiServiceResponse> ValidateRequestAsync(string ItemId);
    }
}
