namespace API_painel_investimentos.DTO.Portfolio;

public record RecommendationRequestDto(
        Guid? UserId = null,
        string? ProfileType = null,
        decimal AvailableAmount = 0
);