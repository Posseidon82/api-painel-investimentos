using API_painel_investimentos.DTO;

namespace API_painel_investimentos.Services.Interfaces;

public interface IProfileCalculationService
{
    (string ProfileType, int Score) CalculateProfile(List<QuestionAnswer> answers);
}
