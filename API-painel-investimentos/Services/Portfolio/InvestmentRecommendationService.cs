using API_painel_investimentos.DTO.Portfolio;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Models.Profile;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Profile.Interfaces;
using API_painel_investimentos.Services.Portfolio.Interfaces;

namespace API_painel_investimentos.Services.Portfolio;

public class InvestmentRecommendationService : IInvestmentRecommendationService
{
    private readonly IInvestmentProductRepository _productRepository;
    private readonly IInvestorProfileRepository _profileRepository;
    private readonly ILogger<InvestmentRecommendationService> _logger;

    public InvestmentRecommendationService(
        IInvestmentProductRepository productRepository,
        IInvestorProfileRepository profileRepository,
        ILogger<InvestmentRecommendationService> logger)
    {
        _productRepository = productRepository;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<RecommendationResultDto> GetRecommendationsAsync(Guid userId)
    {
        _logger.LogInformation("Generating recommendations for user {UserId}", userId);

        // 1. Buscar perfil do usuário do banco
        var profile = await _profileRepository.GetByUserIdAsync(userId);
        if (profile == null)
            throw new NotFoundException($"Perfil não encontrado para o usuário {userId}");

        // 2. Buscar produtos do banco filtrados por perfil
        var recommendedProducts = await _productRepository.GetByProfileAsync(profile.ProfileType);

        // 3. Ordenar por adequação ao perfil
        var orderedProducts = OrderProductsBySuitability(recommendedProducts, profile);

        // 4. Calcular distribuição sugerida
        var allocation = CalculatePortfolioAllocation(profile.ProfileType, orderedProducts);

        _logger.LogInformation("Generated {Count} recommendations for user {UserId} with profile {ProfileType}",
            orderedProducts.Count, userId, profile.ProfileType);

        return new RecommendationResultDto(
            userId,
            profile.ProfileType,
            profile.Score,
            orderedProducts.Take(10).Select(MapToProductDto).ToList(),
            allocation
        );
    }

    public async Task<RecommendationResultDto> GetRecommendationsByProfileAsync(
        string profileType,
        decimal availableAmount)
    {
        _logger.LogInformation("Generating recommendations for profile {ProfileType} with amount {Amount}",
            profileType, availableAmount);

        // Buscar produtos diretamente do banco
        var products = await _productRepository.GetByProfileAsync(profileType);
        var orderedProducts = products
            .Where(p => p.MinimumInvestment <= availableAmount)
            .OrderBy(p => p.MinimumInvestment)
            .ToList();

        var allocation = CalculatePortfolioAllocation(profileType, orderedProducts, availableAmount);

        return new RecommendationResultDto(
            Guid.Empty,
            profileType,
            0, // score não disponível
            orderedProducts.Take(10).Select(MapToProductDto).ToList(),
            allocation
        );
    }

    public async Task<List<InvestmentProduct>> GetProductsByProfileAsync(string profileType)
    {
        // Busca direto do banco via repository
        return await _productRepository.GetByProfileAsync(profileType);
    }

    private List<InvestmentProduct> OrderProductsBySuitability(
        List<InvestmentProduct> products,
        InvestorProfile profile)
    {
        return products
            .OrderBy(p => CalculateSuitabilityScore(p, profile))
            .ThenByDescending(p => p.ExpectedReturn)
            .ToList();
    }

    private int CalculateSuitabilityScore(InvestmentProduct product, InvestorProfile profile)
    {
        var score = 0;

        // Lógica de adequação baseada no perfil vs produto
        switch (profile.ProfileType.ToLower())
        {
            case "conservative":
                score += product.RiskLevel == "Baixo" ? 10 :
                        product.RiskLevel == "Médio" ? 5 : -10;
                break;
            case "moderate":
                score += product.RiskLevel == "Médio" ? 10 :
                        product.RiskLevel == "Baixo" ? 8 :
                        product.RiskLevel == "Alto" ? 3 : 0;
                break;
            case "aggressive":
                score += product.RiskLevel == "Alto" ? 10 :
                        product.RiskLevel == "Médio" ? 7 :
                        product.RiskLevel == "Baixo" ? 2 : 0;
                break;
        }

        // Bonus para produtos com alta liquidez para perfis conservadores
        if (profile.ProfileType == "Conservative" && product.LiquidityDays <= 7)
            score += 5;

        return 100 - score; // Ordenar do mais adequado (menor score) para o menos
    }

    private PortfolioAllocationDto CalculatePortfolioAllocation(
        string profileType,
        List<InvestmentProduct> products,
        decimal availableAmount = 10000m)
    {
        // Lógica de alocação baseada no perfil
        return profileType.ToLower() switch
        {
            "conservative" => new PortfolioAllocationDto(
                ConservativePercentage: 70,
                ModeratePercentage: 25,
                AggressivePercentage: 5,
                SuggestedAmount: availableAmount,
                Description: "Foco em preservação de capital com exposição mínima a riscos. Produtos de renda fixa e tesouro direto."
            ),
            "moderate" => new PortfolioAllocationDto(
                ConservativePercentage: 40,
                ModeratePercentage: 45,
                AggressivePercentage: 15,
                SuggestedAmount: availableAmount,
                Description: "Equilíbrio entre segurança e potencial de retorno. Mistura de renda fixa e fundos balanceados."
            ),
            "aggressive" => new PortfolioAllocationDto(
                ConservativePercentage: 15,
                ModeratePercentage: 35,
                AggressivePercentage: 50,
                SuggestedAmount: availableAmount,
                Description: "Foco em maximização de retorno com tolerância a volatilidade. Ênfase em ações e fundos multimercado."
            ),
            _ => new PortfolioAllocationDto(
                ConservativePercentage: 100,
                ModeratePercentage: 0,
                AggressivePercentage: 0,
                SuggestedAmount: availableAmount,
                Description: "Perfil conservador padrão com foco total em segurança."
            )
        };
    }

    private InvestmentProductDto MapToProductDto(InvestmentProduct product)
    {
        return new InvestmentProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Category,
            product.RiskLevel,
            product.MinimumInvestment,
            product.LiquidityDays,
            product.AdministrationFee,
            product.ExpectedReturn,
            product.Issuer
        );
    }
}
