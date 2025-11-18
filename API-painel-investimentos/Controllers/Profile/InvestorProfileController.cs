using API_painel_investimentos.DTO.Profile;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.Profile.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Profile;

/// <summary>
/// Gerencia os endpoints responsáveis por calcular e recuperar o perfil de investidor
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvestorProfileController : ControllerBase
{
    private readonly IInvestorProfileService _profileService;
    private readonly ILogger<InvestorProfileController> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profileService"></param>
    /// <param name="logger"></param>
    public InvestorProfileController(
        IInvestorProfileService profileService,
        ILogger<InvestorProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    /// <summary>
    /// Calcula o perfil de investidor com base nas respostas do questionário de perfil
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(ProfileResultDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CalculateProfile([FromBody] CalculateProfileRequest request)
    {
        try
        {
            var result = await _profileService.CalculateProfileAsync(request.UserId, request.Answers);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments in CalculateProfile");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating profile");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retorna o perfil de investidor anteriormente calculado para um usuário
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ProfileResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        try
        {
            var result = await _profileService.GetUserProfileAsync(userId);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }
}
