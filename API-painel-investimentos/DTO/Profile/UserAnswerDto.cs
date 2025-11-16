namespace API_painel_investimentos.DTO.Profile;

public record UserAnswerDto(
        Guid QuestionId,
        Guid AnswerOptionId
);