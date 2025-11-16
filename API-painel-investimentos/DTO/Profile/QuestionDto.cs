namespace API_painel_investimentos.DTO.Profile;
public record QuestionDto(
        Guid Id,
        string QuestionText,
        string Category,
        int Weight,
        int Order,
        List<AnswerOptionDto> AnswerOptions
);