namespace API_painel_investimentos.Models
{
    public class QuestionAnswerOption : BaseEntity
    {
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
}
