namespace API_painel_investimentos.Models
{
    public class ProfileAnswer : BaseEntity
    {
        public ProfileAnswer(Guid profileId, Guid questionId, Guid answerOptionId)
        {
            ProfileId = profileId;
            QuestionId = questionId;
            AnswerOptionId = answerOptionId;
            AnsweredAt = DateTime.UtcNow;
        }

        public Guid ProfileId { get; private set; }
        public Guid QuestionId { get; private set; }
        public Guid AnswerOptionId { get; private set; }
        public DateTime AnsweredAt { get; private set; }

        // Navigation properties
        public virtual InvestorProfile Profile { get; private set; }
        public virtual ProfileQuestion Question { get; private set; }
        public virtual QuestionAnswerOption SelectedOption { get; private set; }
    }
}
