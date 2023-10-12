namespace Orchestrator.Contracts.TaxesCalculationEngine;

public class TaxesCalculationResponse
{
    public TaxesCalculationResponse(string itemId, decimal taxAAA, decimal taxBBB, decimal taxCCC)
    {
        ItemId = itemId;
        TaxAAA = taxAAA;
        TaxBBB = taxBBB;
        TaxCCC = taxCCC;
    }

    public TaxesCalculationResponse()
    {
    }

    public string ItemId { get; set; }
    
    public decimal TaxAAA { get; set; }
    
    public decimal TaxBBB { get; set; }
    
    public decimal TaxCCC { get; set; }
}