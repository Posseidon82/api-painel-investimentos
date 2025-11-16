using API_painel_investimentos.Models.Profile;

namespace API_painel_investimentos.DTO.Profile;

internal record ValidatedAnswer(ProfileQuestion Question, QuestionAnswerOption AnswerOption);