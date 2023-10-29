namespace TaxesCalculationEngine.Configuration;
public record KafkaTopics
{
    public string TaxesCalculationEngineRequest { get; set; } = string.Empty;
    public string TaxesCalculationEngineResponse { get; set; } = string.Empty;
    public string DefaultGroup { get; set; } = "taxes.calculation.group";
}