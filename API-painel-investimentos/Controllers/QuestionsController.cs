using API_painel_investimentos.DTO;
using API_painel_investimentos.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IProfileQuestionRepository _questionRepository;

    public QuestionsController(IProfileQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), 200)]
    public async Task<IActionResult> GetActiveQuestions()
    {
        var questions = await _questionRepository.GetActiveQuestionsAsync();

        var questionDtos = questions.Select(q => new QuestionDto(
            q.Id,
            q.QuestionText,
            q.Category,
            q.Weight,
            q.Order,
            q.AnswerOptions.Select(o => new AnswerOptionDto(o.Id, o.OptionText, o.Description)).ToList()
        ));

        return Ok(questionDtos);
    }
}
