namespace API_painel_investimentos.Models.Profile;

/// <summary>
/// Represents an answer option for a specific question, including its text, score, and description.
/// </summary>
/// <remarks>This class is used to define a selectable option for a question, along with its associated
/// metadata. Each option is linked to a specific question via the <see cref="QuestionId"/> property.</remarks>
public class QuestionAnswerOption : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuestionAnswerOption"/> class with the specified question ID,
    /// option text, score, and description.
    /// </summary>
    /// <param name="questionId">The unique identifier of the question associated with this option.</param>
    /// <param name="optionText">The text representing the answer option.</param>
    /// <param name="score">The score assigned to this option, which may be used for evaluation or grading purposes.</param>
    /// <param name="description">A detailed description of the option, providing additional context or clarification.</param>
    public QuestionAnswerOption(Guid questionId, string optionText, int score, string description)
    {
        QuestionId = questionId;
        OptionText = optionText;
        Score = score;
        Description = description;
    }

    public Guid QuestionId { get; private set; }
    public string OptionText { get; private set; }
    public int Score { get; private set; }
    public string Description { get; private set; }

    // Navigation property
    public virtual ProfileQuestion Question { get; private set; }
}