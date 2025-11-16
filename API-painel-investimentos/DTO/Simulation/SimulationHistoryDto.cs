namespace API_painel_investimentos.DTO.Simulation;

public record SimulationHistoryDto(
        Guid SimulationId,
        decimal InvestedAmount,
        int InvestmentMonths,
        decimal TotalAmount,
        decimal ReturnRate,
        DateTime SimulatedAt
);