namespace API_painel_investimentos.Models.Simulation;

public class InvestmentSimulation : BaseEntity
{
    public InvestmentSimulation(
        Guid userId,
        string profileType,
        decimal investedAmount,
        int investmentMonths,
        decimal totalReturn,
        decimal netReturn,
        decimal totalAmount,
        string simulationDetails)
    {
        UserId = userId;
        ProfileType = profileType;
        InvestedAmount = investedAmount;
        InvestmentMonths = investmentMonths;
        TotalReturn = totalReturn;
        NetReturn = netReturn;
        TotalAmount = totalAmount;
        SimulationDetails = simulationDetails;
        SimulatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string ProfileType { get; private set; }
    public decimal InvestedAmount { get; private set; }
    public int InvestmentMonths { get; private set; }
    public decimal TotalReturn { get; private set; } // Rendimento bruto
    public decimal NetReturn { get; private set; }   // Rendimento líquido
    public decimal TotalAmount { get; private set; } // Valor total (investido + líquido)
    public string SimulationDetails { get; private set; } // JSON com detalhes
    public DateTime SimulatedAt { get; private set; }

    // Método para atualizar detalhes (se necessário)
    public void UpdateDetails(string details)
    {
        SimulationDetails = details;
        UpdateTimestamps();
    }
}
