using API_painel_investimentos.DTO;
using API_painel_investimentos.Repositories.Interfaces;
using API_painel_investimentos.Services.Interfaces;

namespace API_painel_investimentos.Services;

public class QuestionService : IQuestionService
{
    private readonly IProfileQuestionRepository _questionRepository;

    public QuestionService(IProfileQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<List<QuestionDto>> GetActiveQuestionsAsync()
    {
        var questions = await _questionRepository.GetActiveQuestionsAsync();

        return questions.Select(q => new QuestionDto(
            q.Id,
            q.QuestionText,
            q.Category,
            q.Weight,
            q.Order,
            q.AnswerOptions.Select(o => new AnswerOptionDto(
                o.Id,
                o.OptionText,
                o.Description
            )).ToList()
        )).ToList();
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(Guid questionId)
    {
        var question = await _questionRepository.GetByIdAsync(questionId);

        if (question == null) return null;

        return new QuestionDto(
            question.Id,
            question.QuestionText,
            question.Category,
            question.Weight,
            question.Order,
            question.AnswerOptions.Select(o => new AnswerOptionDto(
                o.Id,
                o.OptionText,
                o.Description
            )).ToList()
        );
    }
}
