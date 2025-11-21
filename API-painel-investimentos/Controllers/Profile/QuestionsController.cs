using API_painel_investimentos.DTO.Profile;
using API_painel_investimentos.Repositories;
using API_painel_investimentos.Services.Profile.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Profile;

/// <summary>
/// Provides endpoints for managing and retrieving questions.
/// </summary>
/// <remarks>This controller handles operations related to questions, including retrieving active questions and
/// fetching details for a specific question. It interacts with an injected <see cref="IQuestionService"/> to perform
/// the required operations.</remarks>
[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    /// <summary>
    /// Recupera uma lista de questões ativas.
    /// </summary>
    /// <remarks>Este método retorna um coleção de questões que estão atualmente marcadas como ativas. O
    /// resultado é retornado como uma lista de objetos <see cref="QuestionDto"/> .</remarks>
    /// <returns>UM <see cref="IActionResult"/> contendo uma lista de objetos <see cref="QuestionDto"/> representando as questões 
    /// ativas, com um status code 200 (OK).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), 200)]
    [Authorize]
    public async Task<IActionResult> GetActiveQuestions()
    {
        try
        {
            var questions = await _questionService.GetActiveQuestionsAsync();
            return Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active questions");
            return StatusCode(500, "Internal server error");
        }
    }


    /// <summary>
    /// Recupera os dados parametrizados da questão que tem o mesmo questionId fornecido.
    /// </summary>
    /// <param name="questionId">O identificador único da questão associada com o perfil de questões.</param>
    /// <returns>Um <see cref="IActionResult"/> contendo um objeto <see cref="QuestionDto"/> representando a 
    /// questão, com um status code 200 (OK). Um status code 404 (Not Found), se uma correspondeência não for encontrada.</returns>
    [HttpGet("{questionId}")]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> GetQuestion(Guid questionId)
    {
        try
        {
            var question = await _questionService.GetQuestionByIdAsync(questionId);

            if (question == null)
                return NotFound();

            return Ok(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving question by ID: {QuestionId}", questionId);
            return StatusCode(500, "Internal server error");
        }
    }
}
