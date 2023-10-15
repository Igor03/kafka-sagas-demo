namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationRequest
{
    public TaxesCalculationRequest(string itemId)
    {
        ItemId = itemId;
    }

    public Guid CorrelationId { get; set; }
    
    public string ItemId { get; set; }
}