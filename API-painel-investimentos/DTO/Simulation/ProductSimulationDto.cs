namespace API_painel_investimentos.DTO.Simulation;

public record ProductSimulationDto(
        Guid ProductId,
        string ProductName,
        string Category,
        string RiskLevel,
        decimal AllocatedAmount,
        decimal ExpectedReturn,
        decimal GrossReturn,
        decimal Taxes,
        decimal NetReturn,
        decimal FinalAmount,
        string SimulationDetails
);