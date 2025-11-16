using API_painel_investimentos.DTO.Simulation;

namespace API_painel_investimentos.Services.Simulation.Interfaces;

public interface ISimulationStatsService
{
    Task<ProductStatsResponseDto> GetProductDailyStatsAsync(ProductStatsRequestDto request);
    Task<List<ProductDailyStatsDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10);
}
