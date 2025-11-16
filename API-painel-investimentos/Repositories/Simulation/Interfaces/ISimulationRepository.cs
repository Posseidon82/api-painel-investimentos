using API_painel_investimentos.Models.Simulation;

namespace API_painel_investimentos.Repositories.Simulation.Interfaces;

public interface ISimulationRepository
{
    Task<InvestmentSimulation> AddAsync(InvestmentSimulation simulation);
    Task<InvestmentSimulation?> GetByIdAsync(Guid simulationId);
    Task<List<InvestmentSimulation>> GetByUserIdAsync(Guid userId);
    Task<List<InvestmentSimulation>> GetRecentSimulationsAsync(int count = 10);

    // MÉTODOS PARA ESTATÍSTICAS
    Task<List<InvestmentSimulation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<InvestmentSimulation>> GetByProductIdAsync(Guid productId, DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetSimulationCountByDateAsync(DateTime date);
}
