namespace API_painel_investimentos.DTO.Portfolio;

public record InvestmentProductDto(
        Guid Id,
        string Name,
        string Description,
        string Category,
        string RiskLevel,
        decimal MinimumInvestment,
        int LiquidityDays,
        decimal AdministrationFee,
        decimal ExpectedReturn,
        string Issuer
);