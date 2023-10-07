namespace Contracts
{
    public sealed class NotificationReply<TMessage>
        where TMessage: class
    {
        public bool Success { get; set; }
        public string? Reason { get; set; }
        public TMessage? Data { get; set; }
    }
}
