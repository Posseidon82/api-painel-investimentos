using API_painel_investimentos.DTO.Profile;

public record CalculateProfileRequest(
        Guid UserId,
        List<UserAnswerDto> Answers
);