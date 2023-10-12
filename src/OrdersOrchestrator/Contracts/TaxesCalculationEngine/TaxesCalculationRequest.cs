namespace OrdersOrchestrator.Contracts.TaxesCalculationEngine;

public sealed class TaxesCalculationRequest
{
    public TaxesCalculationRequest(string itemId)
    {
        ItemId = itemId;
    }

    public TaxesCalculationRequest()
    {
    }
    
    public string ItemId { get; set; }
}