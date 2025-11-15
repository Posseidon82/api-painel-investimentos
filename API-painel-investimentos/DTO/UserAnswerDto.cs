namespace API_painel_investimentos.DTO;

public record UserAnswerDto(
        Guid QuestionId,
        Guid AnswerOptionId
);