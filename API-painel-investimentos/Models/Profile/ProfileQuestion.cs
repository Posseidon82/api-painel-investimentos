namespace API_painel_investimentos.Models.Profile;

/// <summary>
/// Represents a question used in a user profile, including its text, category, weight, order, and associated answer
/// options.
/// </summary>
/// <remarks>A <see cref="ProfileQuestion"/> is typically used to gather user input in areas such as risk
/// tolerance, objectives, or knowledge. Each question has a weight that determines its relative importance, an
/// order for display purposes, and a collection of answer options. The question can be activated or deactivated as
/// needed.</remarks>
public class ProfileQuestion : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileQuestion"/> class with the specified question text,
    /// category, weight, and order.
    /// </summary>
    /// <remarks>The <see cref="IsActive"/> property is initialized to <see langword="true"/> by
    /// default.</remarks>
    /// <param name="questionText">The text of the question. This value cannot be null or empty.</param>
    /// <param name="category">The category to which the question belongs. This value cannot be null or empty.</param>
    /// <param name="weight">The weight of the question, which determines its relative importance. Must be a non-negative integer.</param>
    /// <param name="order">The order in which the question should appear. Must be a non-negative integer.</param>
    public ProfileQuestion(string questionText, string category, int weight, int order)
    {
        //QuestionId = Guid.NewGuid();
        QuestionText = questionText;
        Category = category;
        Weight = weight;
        Order = order;
        IsActive = true;
    }

    private readonly Guid QuestionId;

    public string QuestionText { get; private set; }
    public string Category { get; private set; } // RiskTolerance, Objectives, Knowledge, etc.
    public int Weight { get; private set; } // 1-100
    public int Order { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ICollection<QuestionAnswerOption> AnswerOptions { get; private set; } = new List<QuestionAnswerOption>();

    public void AddAnswerOption(string optionText, int score, string description)
    {
        AnswerOptions.Add(new QuestionAnswerOption(Id, optionText, score, description));
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
