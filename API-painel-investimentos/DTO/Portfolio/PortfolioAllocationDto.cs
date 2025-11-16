namespace API_painel_investimentos.DTO.Portfolio;

public record PortfolioAllocationDto(
        int ConservativePercentage = 0,
        int ModeratePercentage = 0,
        int AggressivePercentage = 0,
        decimal SuggestedAmount = 0,
        string Description = ""
);