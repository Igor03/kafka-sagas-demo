namespace CustomerValidationEngine.Configuration
{
    public record KafkaTopics
    {
        public string CustomerValidationEngineRequest { get; set; } = string.Empty;
        public string CustomerValidationEngineResponse { get; set; } = string.Empty;
        public string DefaultGroup { get; set; } = "customer.validation.group";
    }
}
