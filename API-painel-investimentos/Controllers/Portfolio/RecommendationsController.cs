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
