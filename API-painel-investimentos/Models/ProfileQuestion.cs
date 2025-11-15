namespace API_painel_investimentos.Models
{
    public class ProfileQuestion : BaseEntity
    {
        public ProfileQuestion(string questionText, string category, int weight, int order)
        {
            QuestionText = questionText;
            Category = category;
            Weight = weight;
            Order = order;
            IsActive = true;
        }

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
}
