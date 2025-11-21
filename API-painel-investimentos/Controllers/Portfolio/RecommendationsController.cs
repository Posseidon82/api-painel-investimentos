using API_painel_investimentos.DTO.Portfolio;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.Portfolio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Portfolio;

// API/Controllers/RecommendationsController.cs
[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IInvestmentRecommendationService _recommendationService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        IInvestmentRecommendationService recommendationService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    /// <summary>
    /// Recupera uma lista de produtos de investimento adequadas ao perfil de investidor calculado
    /// a partir das respostas do usuário ao questionário de avaliação de perfil de investidor
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(RecommendationResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserRecommendations(Guid userId)
    {
        try
        {
            var recommendations = await _recommendationService.GetRecommendationsAsync(userId);
            return Ok(recommendations);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User profile not found for recommendations");
            return NotFound(ex.Message);
        }
    }


    /// <summary>
    /// Retorna os produtos de investimento recomendados de acordo com o perfil de investidor e o montante a ser investido
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("profile-based")]
    [ProducesResponseType(typeof(RecommendationResultDto), 200)]
    public async Task<IActionResult> GetRecommendationsByProfile([FromBody] RecommendationRequestDto request)
    {
        try
        {
            RecommendationResultDto recommendations;

            if (request.UserId.HasValue)
            {
                recommendations = await _recommendationService.GetRecommendationsAsync(request.UserId.Value);
            }
            else if (!string.IsNullOrEmpty(request.ProfileType))
            {
                recommendations = await _recommendationService.GetRecommendationsByProfileAsync(
                    request.ProfileType,
                    request.AvailableAmount);
            }
            else
            {
                return BadRequest("Must provide either UserId or ProfileType");
            }

            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating investment recommendations");
            return StatusCode(500, "Internal server error");
        }
    }


    /// <summary>
    /// Retorna a lista de produtos de investimento disponíveis de acordo com o perfil de investimento informado: 
    /// Conservative, Moderate, Agressive
    /// </summary>
    /// <param name="profileType"></param>
    /// <returns></returns>
    [HttpGet("products/{profileType}")]
    [ProducesResponseType(typeof(List<InvestmentProductDto>), 200)]
    public async Task<IActionResult> GetProductsByProfile(string profileType)
    {
        try
        {
            var products = await _recommendationService.GetProductsByProfileAsync(profileType);
            var productDtos = products.Select(p => new InvestmentProductDto(
                p.Id, p.Name, p.Description, p.Category, p.RiskLevel,
                p.MinimumInvestment, p.LiquidityDays, p.AdministrationFee,
                p.ExpectedReturn, p.Issuer
            )).ToList();

            return Ok(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for profile {ProfileType}", profileType);
            return StatusCode(500, "Internal server error");
        }
    }
}
