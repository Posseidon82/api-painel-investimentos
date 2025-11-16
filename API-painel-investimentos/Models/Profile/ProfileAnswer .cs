namespace API_painel_investimentos.Models.Profile
{
    /// <summary>
    /// Represents an answer provided by a user to a specific question within a profile,  including the selected answer
    /// option and the time the answer was recorded.
    /// </summary>
    /// <remarks>This class is used to associate a user's profile with their response to a specific question 
    /// and the selected answer option. It also tracks the timestamp of when the answer was provided.</remarks>
    public class ProfileAnswer : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileAnswer"/> class with the specified profile, question,
        /// and answer option identifiers.
        /// </summary>
        /// <param name="profileId">The unique identifier of the profile associated with this answer.</param>
        /// <param name="questionId">The unique identifier of the question being answered.</param>
        /// <param name="answerOptionId">The unique identifier of the selected answer option.</param>
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
