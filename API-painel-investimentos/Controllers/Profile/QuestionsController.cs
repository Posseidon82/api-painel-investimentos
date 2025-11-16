using API_painel_investimentos.DTO.Profile;
using API_painel_investimentos.Repositories;
using API_painel_investimentos.Services.Profile.Interfaces;
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

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    /// <summary>
    /// Retrieves a list of active questions.
    /// </summary>
    /// <remarks>This method returns a collection of questions that are currently marked as active.  The
    /// result is returned as a list of <see cref="QuestionDto"/> objects.</remarks>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="QuestionDto"/> objects  representing the active
    /// questions, with a status code of 200 (OK).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestionDto>), 200)]
    public async Task<IActionResult> GetActiveQuestions()
    {
        var questions = await _questionService.GetActiveQuestionsAsync();
        return Ok(questions);
    }

    /// <summary>
    /// Retrieves the <see cref="QuestionDto"/> object that has the same questionId as the one provided.
    /// </summary>
    /// <param name="questionId">The unique identifier of the question associated with the question profile.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="QuestionDto"/> object  representing the 
    /// question, with a status code of 200 (OK). A status code 404 (Not Found), if no match is found.</returns>
    [HttpGet("{questionId}")]
    [ProducesResponseType(typeof(QuestionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetQuestion(Guid questionId)
    {
        var question = await _questionService.GetQuestionByIdAsync(questionId);

        if (question == null)
            return NotFound();

        return Ok(question);
    }
}
