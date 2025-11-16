using API_painel_investimentos.DTO.Simulation;

namespace API_painel_investimentos.Services.Simulation;

internal class SimulationDetails
{
    public List<ProductSimulationDto> ProductSimulations { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}
