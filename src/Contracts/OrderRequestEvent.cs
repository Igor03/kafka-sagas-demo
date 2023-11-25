namespace Contracts;

public record OrderRequestEvent
{
    public string CustomerId { get; set;} = default!;
    public string ItemId { get; set; } = default!;
}