using API_painel_investimentos.DTO.Simulation;

namespace API_painel_investimentos.Services.Simulation.Interfaces;

public interface IInvestmentSimulationService
{
    Task<SimulationResultDto> SimulateInvestmentAsync(SimulationRequestDto request);
    Task<List<SimulationHistoryDto>> GetUserSimulationsAsync(Guid userId);
    Task<SimulationResultDto?> GetSimulationByIdAsync(Guid simulationId);
}
