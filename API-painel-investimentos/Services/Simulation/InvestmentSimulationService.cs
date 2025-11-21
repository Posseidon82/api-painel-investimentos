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

    /// <summary>
    /// Simula a aplicação do montante, durante o período informado, nos produtos informados
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
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


    /// <summary>
    /// Recupera a lista de simulações de um usuário a partir de seu userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Recupera uma simulação pelo seu id
    /// </summary>
    /// <param name="simulationId"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Faz a validação do preenchimento dos dados de montante e prazo de investimento
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="ArgumentException"></exception>
    private void ValidateSimulationRequest(SimulationRequestDto request)
    {
        if (request.InvestedAmount <= 0)
            throw new ArgumentException("O valor investido deve ser maior que zero");

        if (request.InvestmentMonths <= 0)
            throw new ArgumentException("O período de investimento deve ser maior que zero");

        if (request.InvestmentMonths > 360) // 30 anos
            throw new ArgumentException("O período de investimento não pode exceder 360 meses");
    }


    /// <summary>
    /// Recupera os parâmetros dos produtos de investimentos indicados ou, caso não informado algum 
    /// retorna os produtos de investimento referentes ao mesmo perfil de investidor do usuário
    /// </summary>
    /// <param name="request"></param>
    /// <param name="profileType"></param>
    /// <returns>Retorna uma lista com os parâmetros dos produtos de investimento</returns>
    private async Task<List<InvestmentProduct>> GetProductsForSimulation(SimulationRequestDto request, string profileType)
    {
        //Verifica se foi passado id de algum produto
        if (request.ProductIds?.Any() == true)
        {
            var products = new List<InvestmentProduct>(); // Cria lista para guardar os parametros dos produtos de investimento
            foreach (var productId in request.ProductIds)
            {
                // Recupera os parâmetros do produto indicado
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


    /// <summary>
    /// Calcula a alocação do montante entre os produtos de investimento com base no perfil do investidor
    /// </summary>
    /// <param name="products"></param>
    /// <param name="totalAmount"></param>
    /// <param name="profileType"></param>
    /// <returns></returns>
    private List<ProductAllocation> CalculateProductAllocation(List<InvestmentProduct> products, decimal totalAmount, string profileType)
    {
        var allocations = new List<ProductAllocation>(); // Parametros de produto e montante

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


    /// <summary>
    /// Retorna a distribuição percentual dos produtos com base no perfil do investidor
    /// </summary>
    /// <param name="profileType"></param>
    /// <param name="productCount"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Executa a simulação de investimento para a alocação de produto informada
    /// </summary>
    /// <param name="allocation"></param>
    /// <param name="months"></param>
    /// <returns></returns>
    private ProductSimulationDto SimulateProductInvestment(ProductAllocation allocation, int months)
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


    /// <summary>
    /// Retorna a alíquota de imposto de renda com base na tabela regressiva
    /// </summary>
    /// <param name="months"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Salva a simulação no banco de dados
    /// </summary>
    /// <param name="request"></param>
    /// <param name="profile"></param>
    /// <param name="totalGrossReturn"></param>
    /// <param name="totalNetReturn"></param>
    /// <param name="totalAmount"></param>
    /// <param name="productSimulations"></param>
    /// <returns></returns>
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