using API_painel_investimentos.Models.Portfolio;

namespace API_painel_investimentos.Repositories.Portfolio.Interfaces;

public interface IInvestmentProductRepository
{
    Task<List<InvestmentProduct>> GetActiveProductsAsync();
    Task<InvestmentProduct?> GetByIdAsync(Guid id);
    Task<List<InvestmentProduct>> GetByCategoryAsync(string category);
    Task<List<InvestmentProduct>> GetByRiskLevelAsync(string riskLevel);
    Task<List<InvestmentProduct>> GetByProfileAsync(string targetProfile);
    Task UpdateProductAsync(InvestmentProduct product);
}
