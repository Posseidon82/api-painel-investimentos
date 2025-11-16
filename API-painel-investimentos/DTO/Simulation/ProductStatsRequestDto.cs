namespace API_painel_investimentos.DTO.Simulation;

public record ProductStatsRequestDto(
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        Guid? ProductId = null,
        string? Category = null
);