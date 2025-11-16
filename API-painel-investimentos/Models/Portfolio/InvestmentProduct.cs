namespace API_painel_investimentos.Models.Portfolio;

public class InvestmentProduct : BaseEntity
{
    public InvestmentProduct(
        string name,
        string description,
        string category,
        string riskLevel,
        decimal minimumInvestment,
        int liquidityDays,
        string targetProfile,
        decimal administrationFee,
        decimal expectedReturn,
        string issuer)
    {
        Name = name;
        Description = description;
        Category = category;
        RiskLevel = riskLevel;
        MinimumInvestment = minimumInvestment;
        LiquidityDays = liquidityDays;
        TargetProfile = targetProfile;
        AdministrationFee = administrationFee;
        ExpectedReturn = expectedReturn;
        Issuer = issuer;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; } // RendaFixa, TesouroDireto, Fundos, etc.
    public string RiskLevel { get; private set; } // Baixo, Médio, Alto
    public decimal MinimumInvestment { get; private set; }
    public int LiquidityDays { get; private set; } // Dias para resgate
    public string TargetProfile { get; private set; } // Conservative, Moderate, Aggressive
    public decimal AdministrationFee { get; private set; }
    public decimal ExpectedReturn { get; private set; } // % ao ano
    public string Issuer { get; private set; }
    public bool IsActive { get; private set; }

    // Métodos
    public void UpdateProduct(
        string description,
        decimal administrationFee,
        decimal expectedReturn)
    {
        Description = description;
        AdministrationFee = administrationFee;
        ExpectedReturn = expectedReturn;
        UpdateTimestamps();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamps();
    }
}
