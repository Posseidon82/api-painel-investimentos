using API_painel_investimentos.Models;

namespace API_painel_investimentos.Repositories;

public interface IInvestorProfileRepository
{
    Task<InvestorProfile?> GetByUserIdAsync(Guid userId);
    Task<InvestorProfile> CreateAsync(InvestorProfile profile);
    Task UpdateAsync(InvestorProfile profile);
    Task<bool> ExistsForUserAsync(Guid userId);
}
