namespace OrdersOrchestrator.Configuration;

public record KafkaTopics
{
    public string OrderManagementSystemRequest { get; set; } = string.Empty;
    public string OrderManagementSystemResponse { get; set; } = string.Empty;
    public string TaxesCalculationEngineRequest { get; set; } = string.Empty;
    public string TaxesCalculationEngineResponse { get; set; } = string.Empty;
    public string DefaultGroup { get; set; } = "orders_orchestrator.group";
}