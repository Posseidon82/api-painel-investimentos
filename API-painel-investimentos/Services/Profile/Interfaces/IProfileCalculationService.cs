using API_painel_investimentos.DTO.Profile;

namespace API_painel_investimentos.Services.Profile.Interfaces;

public interface IProfileCalculationService
{
    (string ProfileType, int Score) CalculateProfile(List<QuestionAnswer> answers);
}
