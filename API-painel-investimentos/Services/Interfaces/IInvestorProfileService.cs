using API_painel_investimentos.DTO;

namespace API_painel_investimentos.Services.Interfaces;

public interface IInvestorProfileService
{
    Task<ProfileResultDto> CalculateProfileAsync(Guid userId, List<UserAnswerDto> answers);
    Task<ProfileResultDto> GetUserProfileAsync(Guid userId);
    Task<bool> ProfileExistsAsync(Guid userId);
}
