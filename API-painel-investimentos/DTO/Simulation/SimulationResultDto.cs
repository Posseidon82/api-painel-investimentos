namespace API_painel_investimentos.DTO.Simulation;

public record SimulationResultDto(
        Guid SimulationId,
        Guid UserId,
        string ProfileType,
        decimal InvestedAmount,
        int InvestmentMonths,
        decimal TotalReturn,
        decimal NetReturn,
        decimal TotalAmount,
        decimal ReturnRate, // Percentual de retorno
        List<ProductSimulationDto> ProductSimulations,
        DateTime SimulatedAt
);