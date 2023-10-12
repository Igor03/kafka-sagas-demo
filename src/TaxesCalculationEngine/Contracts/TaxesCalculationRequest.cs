namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationRequest
{
    public TaxesCalculationRequest(string itemId)
    {
        ItemId = itemId;
    }
    
    public string ItemId { get; set; }
}