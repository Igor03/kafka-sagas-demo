namespace Producer.Configuration;

public record KafkaTopics
{
    public string SourceSystem { get; set; } = string.Empty;
}