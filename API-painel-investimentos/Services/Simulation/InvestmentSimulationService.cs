using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Models.Profile;
using API_painel_investimentos.Models.Simulation;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Profile.Interfaces;
using API_painel_investimentos.Repositories.Simulation.Interfaces;
using API_painel_investimentos.Services.Simulation.Interfaces;
using System.Text.Json;

namespace API_painel_investimentos.Services.Simulation;

public class InvestmentSimulationService : IInvestmentSimulationService
{
    private readonly IInvestmentProductRepository _productRepository;
    private readonly IInvestorProfileRepository _profileRepository;
    private readonly ISimulationRepository _simulationRepository;
    private readonly ILogger<InvestmentSimulationService> _logger;

    public InvestmentSimulationService(
        IInvestmentProductRepository productRepository,
        IInvestorProfileRepository profileRepository,
        ISimulationRepository simulationRepository,
        ILogger<InvestmentSimulationService> logger)
    {
        _productRepository = productRepository;
        _profileRepository = profileRepository;
        _simulationRepository = simulationRepository;
        _logger = logger;
    }

    public async Task<SimulationResultDto> SimulateInvestmentAsync(SimulationRequestDto request)
    {
        _logger.LogInformation(
            "Starting investment simulation for user {UserId}: Amount {Amount}, Months {Months}",
            request.UserId, request.InvestedAmount, request.InvestmentMonths);

        // 1. Validar request
        ValidateSimulationRequest(request);

        // 2. Buscar perfil do usuário
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId);
        if (profile == null)
            throw new NotFoundException($"Perfil não encontrado para o usuário {request.UserId}");

        // 3. Buscar produtos para simulação
        var products = await GetProductsForSimulation(request, profile.ProfileType);

        // 4. Calcular alocação por produto
        var productAllocations = CalculateProductAllocation(products, request.InvestedAmount, profile.ProfileType);

        // 5. Simular cada produto
        var productSimulations = new List<ProductSimulationDto>();
        decimal totalGrossReturn = 0;
        decimal totalNetReturn = 0;

        foreach (var allocation in productAllocations)
        {
            var simulation = SimulateProductInvestment(allocation, request.InvestmentMonths);
            productSimulations.Add(simulation);

            totalGrossReturn += simulation.GrossReturn;
            totalNetReturn += simulation.NetReturn;
        }

        // 6. Calcular totais
        var totalAmount = request.InvestedAmount + totalNetReturn;
        var returnRate = (totalNetReturn / request.InvestedAmount) * 100;

        // 7. Salvar simulação no banco
        var simulationEntity = await SaveSimulationAsync(
            request, profile, totalGrossReturn, totalNetReturn, totalAmount, productSimulations);

        _logger.LogInformation(
            "Investment simulation completed for user {UserId}. Simulation ID: {SimulationId}",
            request.UserId, simulationEntity.Id);

        // 8. Retornar resultado
        return new SimulationResultDto(
            simulationEntity.Id,
            request.UserId,
            profile.ProfileType,
            request.InvestedAmount,
            request.InvestmentMonths,
            totalGrossReturn,
            totalNetReturn,
            totalAmount,
            returnRate,
            productSimulations,
            simulationEntity.SimulatedAt
        );
    }

    public async Task<List<SimulationHistoryDto>> GetUserSimulationsAsync(Guid userId)
    {
        var simulations = await _simulationRepository.GetByUserIdAsync(userId);

        return simulations.Select(s => new SimulationHistoryDto(
            s.Id,
            s.InvestedAmount,
            s.InvestmentMonths,
            s.TotalAmount,
            (s.NetReturn / s.InvestedAmount) * 100,
            s.SimulatedAt
        )).ToList();
    }

    public async Task<SimulationResultDto?> GetSimulationByIdAsync(Guid simulationId)
    {
        var simulation = await _simulationRepository.GetByIdAsync(simulationId);
        if (simulation == null) return null;

        // Desserializar os detalhes da simulação
        var details = JsonSerializer.Deserialize<SimulationDetails>(simulation.SimulationDetails);

        return new SimulationResultDto(
            simulation.Id,
            simulation.UserId,
            simulation.ProfileType,
            simulation.InvestedAmount,
            simulation.InvestmentMonths,
            simulation.TotalReturn,
            simulation.NetReturn,
            simulation.TotalAmount,
            (simulation.NetReturn / simulation.InvestedAmount) * 100,
            details?.ProductSimulations ?? new List<ProductSimulationDto>(),
            simulation.SimulatedAt
        );
    }

    private void ValidateSimulationRequest(SimulationRequestDto request)
    {
        if (request.InvestedAmount <= 0)
            throw new ArgumentException("O valor investido deve ser maior que zero");

        if (request.InvestmentMonths <= 0)
            throw new ArgumentException("O período de investimento deve ser maior que zero");

        if (request.InvestmentMonths > 360) // 30 anos
            throw new ArgumentException("O período de investimento não pode exceder 360 meses");
    }

    private async Task<List<InvestmentProduct>> GetProductsForSimulation(
        SimulationRequestDto request, string profileType)
    {
        if (request.ProductIds?.Any() == true)
        {
            // Simular produtos específicos
            var products = new List<InvestmentProduct>();
            foreach (var productId in request.ProductIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null && product.IsActive)
                    products.Add(product);
            }
            return products;
        }
        else
        {
            // Usar produtos recomendados para o perfil
            return await _productRepository.GetByProfileAsync(profileType);
        }
    }

    private List<ProductAllocation> CalculateProductAllocation(
        List<InvestmentProduct> products, decimal totalAmount, string profileType)
    {
        var allocations = new List<ProductAllocation>();

        // Distribuição baseada no perfil
        var distribution = GetProfileDistribution(profileType, products.Count);

        for (int i = 0; i < products.Count && i < distribution.Length; i++)
        {
            var allocationPercentage = (decimal)distribution[i] / 100m;
            var allocatedAmount = totalAmount * allocationPercentage / 100;

            // Garantir que atinge o investimento mínimo
            if (allocatedAmount < products[i].MinimumInvestment)
            {
                allocatedAmount = products[i].MinimumInvestment;
            }

            allocations.Add(new ProductAllocation(products[i], allocatedAmount));
        }

        // Ajustar para totalizar 100%
        var totalAllocated = allocations.Sum(a => a.AllocatedAmount);
        if (totalAllocated > totalAmount)
        {
            // Proporcionalmente reduzir
            var ratio = totalAmount / totalAllocated;
            foreach (var allocation in allocations)
            {
                allocation.AllocatedAmount *= ratio;
            }
        }

        return allocations;
    }

    private float[] GetProfileDistribution(string profileType, int productCount)
    {
        return profileType.ToLower() switch
        {
            "conservative" => new[] { 40f, 30f, 20f, 10f }, // 4 produtos
            "moderate" => new[] { 30f, 25f, 20f, 15f, 10f }, // 5 produtos
            "aggressive" => new[] { 25f, 20f, 18f, 15f, 12f, 10f }, // 6 produtos
            _ => new[] { 100f } // padrão: 1 produto
        };
    }

    private ProductSimulationDto SimulateProductInvestment(
        ProductAllocation allocation, int months)
    {
        var product = allocation.Product;

        // Calcular rendimento bruto
        var monthlyRate = (double)product.ExpectedReturn;
        var grossReturn = allocation.AllocatedAmount * (decimal)Math.Pow(1 + monthlyRate, months) - allocation.AllocatedAmount;

        // Calcular impostos (regressivo)
        var taxRate = CalculateTaxRate(months);
        var taxes = grossReturn * taxRate;

        // Calcular rendimento líquido
        var netReturn = grossReturn - taxes;
        var finalAmount = allocation.AllocatedAmount + netReturn;

        return new ProductSimulationDto(
            product.Id,
            product.Name,
            product.Category,
            product.RiskLevel,
            allocation.AllocatedAmount,
            product.ExpectedReturn,
            grossReturn,
            taxes,
            netReturn,
            finalAmount,
            $"Taxa: {product.ExpectedReturn:P2} | IR: {taxRate:P2} | Meses: {months}"
        );
    }

    private decimal CalculateTaxRate(int months)
    {
        // Tabela regressiva do IR para renda fixa
        return months switch
        {
            <= 6 => 0.225m,   // 22.5%
            <= 12 => 0.20m,   // 20%
            <= 24 => 0.175m,  // 17.5%
            _ => 0.15m        // 15%
        };
    }

    private async Task<InvestmentSimulation> SaveSimulationAsync(
        SimulationRequestDto request,
        InvestorProfile profile,
        decimal totalGrossReturn,
        decimal totalNetReturn,
        decimal totalAmount,
        List<ProductSimulationDto> productSimulations)
    {
        var simulationDetails = new SimulationDetails
        {
            ProductSimulations = productSimulations,
            CalculatedAt = DateTime.UtcNow
        };

        var simulationJson = JsonSerializer.Serialize(simulationDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        var simulation = new InvestmentSimulation(
            request.UserId,
            profile.ProfileType,
            request.InvestedAmount,
            request.InvestmentMonths,
            totalGrossReturn,
            totalNetReturn,
            totalAmount,
            simulationJson
        );

        return await _simulationRepository.AddAsync(simulation);
    }
}