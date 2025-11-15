namespace API_painel_investimentos.DTO;

public record ProfileResultDto(
        Guid UserId,
        string ProfileType,
        int Score,
        DateTime CalculatedAt,
        List<UserAnswerDetailDto> Answers
);