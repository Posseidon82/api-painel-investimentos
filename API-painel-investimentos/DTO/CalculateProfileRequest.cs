using API_painel_investimentos.DTO;

public record CalculateProfileRequest(
        Guid UserId,
        List<UserAnswerDto> Answers
);