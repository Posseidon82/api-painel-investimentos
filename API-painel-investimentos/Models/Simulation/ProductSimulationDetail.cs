namespace API_painel_investimentos.Models.Simulation;

internal class ProductSimulationDetail
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal GrossReturn { get; set; }
    public decimal NetReturn { get; set; }
    public DateTime SimulationDate { get; set; }
}
