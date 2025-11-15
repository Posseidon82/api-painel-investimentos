using API_painel_investimentos.Models;

namespace API_painel_investimentos.Repositories;

public interface IProfileQuestionRepository
{
    Task<IEnumerable<ProfileQuestion>> GetActiveQuestionsAsync();
    Task<ProfileQuestion?> GetByIdAsync(Guid id);
    Task<QuestionAnswerOption?> GetAnswerOptionByIdAsync(Guid optionId);
    Task<bool> QuestionExistsAsync(Guid questionId);
}
