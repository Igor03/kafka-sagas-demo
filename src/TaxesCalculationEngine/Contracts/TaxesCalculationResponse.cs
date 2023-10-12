namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationResponse
{
    public TaxesCalculationResponse(string itemId, decimal taxAaa, decimal taxBbb, decimal taxCcc)
    {
        ItemId = itemId;
        TaxAAA = taxAaa;
        TaxBBB = taxBbb;
        TaxCCC = taxCcc;
    }

    public TaxesCalculationResponse()
    {
    }

    public string ItemId { get; set; }
    
    public decimal TaxAAA { get; set; }
    
    public decimal TaxBBB { get; set; }
    
    public decimal TaxCCC { get; set; }
}