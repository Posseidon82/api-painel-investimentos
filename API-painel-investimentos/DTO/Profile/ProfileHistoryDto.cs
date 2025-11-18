namespace API_painel_investimentos.DTO.Profile;

public record ProfileHistoryDto(
        Guid ProfileId,
        string ProfileType,
        int Score,
        DateTime CalculatedAt,
        DateTime? UpdatedAt
);