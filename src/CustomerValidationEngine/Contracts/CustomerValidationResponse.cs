﻿namespace CustomerValidationEngine.Contracts
{
    public sealed class CustomerValidationResponse
    {
        public Guid CorrelationId { get; set; }

        public string AdjudtedCustomerId { get; set; }
    }
}