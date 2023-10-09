namespace Orchestrator.Configuration;

public record KafkaTopics
{
    public string SourceSystemRequest { get; set; } = string.Empty;
    public string SourceSystemResponse { get; set; } = string.Empty;
    public string ConsumerXRequest { get; set; } = string.Empty;
    public string ConsumerXResponse { get; set; } = string.Empty;
    public string DefaultGroup { get; set; } = "default.group";
}