using OrdersOrchestrator.Contracts.ApiService;

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

        public Task<bool> ValidateIncomingRequestAsync()
        {
            throw new NotImplementedException();
        }
    }
}
