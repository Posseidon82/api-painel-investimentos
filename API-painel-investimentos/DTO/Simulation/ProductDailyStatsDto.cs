namespace API_painel_investimentos.DTO.Simulation;

public record ProductDailyStatsDto(
        Guid ProductId,
        string ProductName,
        string Category,
        string RiskLevel,
        DateTime Date,
        int SimulationCount,
        decimal AverageFinalAmount,
        decimal TotalInvestedAmount,
        decimal MinFinalAmount,
        decimal MaxFinalAmount,
        decimal AverageReturnRate
);