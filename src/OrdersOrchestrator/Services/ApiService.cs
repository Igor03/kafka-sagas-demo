using OrdersOrchestrator.Contracts.ApiService;

namespace OrdersOrchestrator.Services
{
    public sealed class ApiService : IApiService
    {
        public Task<ApiServiceResponse> ValidateRequestAsync(string ItemId)
        {
            return Task.FromResult(new ApiServiceResponse
            {
                AdjustedName = $"validated-{ItemId}",
                Valid = true,
            });
        }
    }
}
