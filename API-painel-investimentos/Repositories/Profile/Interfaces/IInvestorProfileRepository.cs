using API_painel_investimentos.Models.Profile;

namespace API_painel_investimentos.Repositories.Profile.Interfaces;

public interface IInvestorProfileRepository
{
    Task<InvestorProfile?> GetByUserIdAsync(Guid userId);
    Task<InvestorProfile> CreateAsync(InvestorProfile profile);
    Task UpdateAsync(InvestorProfile profile);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task AddProfileAnswerAsync(ProfileAnswer answer);
    Task ClearProfileAnswersAsync(Guid profileId);
    Task<List<ProfileAnswer>> GetProfileAnswersAsync(Guid profileId);
}
