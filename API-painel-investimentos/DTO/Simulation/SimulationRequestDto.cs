namespace API_painel_investimentos.DTO.Simulation;

public record SimulationRequestDto(
        Guid UserId,
        decimal InvestedAmount,
        int InvestmentMonths,
        List<Guid>? ProductIds = null // Opcional: simular produtos específicos
);