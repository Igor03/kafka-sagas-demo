namespace TaxesCalculationEngine.Contracts;

public class TaxesCalculationResponse
{
    public Guid CorrelationId { get; set; }
    
    public decimal TaxAAA { get; set; }
    
    public decimal TaxBBB { get; set; }
    
    public decimal TaxCCC { get; set; }
}