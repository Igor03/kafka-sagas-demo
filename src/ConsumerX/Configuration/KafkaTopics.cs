namespace ConsumerX.Configuration
{
    public record KafkaTopics
    {
        public string ConsumerXRequest { get; set; } = string.Empty;
        public string ConsumerXResponse { get; set; } = string.Empty;
        public string DefaultGroup { get; set; } = "default.group";
    }
}
