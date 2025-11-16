using API_painel_investimentos.Models.Portfolio;

namespace API_painel_investimentos.Services.Simulation;

internal class ProductAllocation
{
    public InvestmentProduct Product { get; }
    public decimal AllocatedAmount { get; set; }

    public ProductAllocation(InvestmentProduct product, decimal allocatedAmount)
    {
        Product = product;
        AllocatedAmount = allocatedAmount;
    }
}
