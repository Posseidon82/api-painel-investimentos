using API_painel_investimentos.DTO.Profile;

namespace API_painel_investimentos.Services.Profile.Interfaces;

public interface IInvestorProfileService
{
    Task<ProfileResultDto> CalculateProfileAsync(Guid userId, List<UserAnswerDto> answers);
    Task<ProfileResultDto> GetUserProfileAsync(Guid userId);
    Task<bool> ProfileExistsAsync(Guid userId);
    // Atualizar perfil existente
    //Task<ProfileResultDto> UpdateProfileAsync(Guid userId, List<UserAnswerDto> answers);

    // Obter histórico de perfis
    //Task<List<ProfileHistoryDto>> GetProfileHistoryAsync(Guid userId);
}
