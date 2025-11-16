namespace API_painel_investimentos.DTO.Simulation;

public record ProductStatsResponseDto(
        DateTime StartDate,
        DateTime EndDate,
        int TotalSimulations,
        int UniqueProducts,
        List<ProductDailyStatsDto> DailyStats
);