using API_painel_investimentos.DTO;

namespace API_painel_investimentos.Services.Interfaces;

public interface IQuestionService
{
    Task<List<QuestionDto>> GetActiveQuestionsAsync();
    Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId);
}
