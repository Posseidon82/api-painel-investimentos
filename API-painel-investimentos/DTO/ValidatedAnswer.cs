using API_painel_investimentos.Models;

namespace API_painel_investimentos.DTO;

internal record ValidatedAnswer(ProfileQuestion Question, QuestionAnswerOption AnswerOption);