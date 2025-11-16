using API_painel_investimentos.DTO.Profile;

namespace API_painel_investimentos.Services.Profile.Interfaces;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetActiveQuestionsAsync();
    Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId);
}
