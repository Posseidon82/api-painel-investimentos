using API_painel_investimentos.DTO.Portfolio;
using API_painel_investimentos.Models.Portfolio;

namespace API_painel_investimentos.Services.Portfolio.Interfaces;

public interface IInvestmentRecommendationService
{
    Task<RecommendationResultDto> GetRecommendationsAsync(Guid userId);
    Task<RecommendationResultDto> GetRecommendationsByProfileAsync(string profileType, decimal availableAmount);
    Task<List<InvestmentProduct>> GetProductsByProfileAsync(string profileType);
}
