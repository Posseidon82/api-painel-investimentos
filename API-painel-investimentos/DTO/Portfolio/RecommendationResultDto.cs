namespace API_painel_investimentos.DTO.Portfolio;

public record RecommendationResultDto(
        Guid UserId,
        string ProfileType,
        int ProfileScore,
        List<InvestmentProductDto> RecommendedProducts,
        PortfolioAllocationDto SuggestedAllocation
);