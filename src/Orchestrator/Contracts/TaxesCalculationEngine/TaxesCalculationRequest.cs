namespace Orchestrator.Contracts.TaxesCalculationEngine;

public class TaxesCalculationRequest
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