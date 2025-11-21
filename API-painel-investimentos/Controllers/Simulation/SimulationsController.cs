using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.Simulation.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Simulation;

[ApiController]
[Route("api/[controller]/[action]")]
public class SimulationsController : ControllerBase
{
    private readonly IInvestmentSimulationService _simulationService;
    private readonly ILogger<SimulationsController> _logger;

    public SimulationsController(
        IInvestmentSimulationService simulationService,
        ILogger<SimulationsController> logger)
    {
        _simulationService = simulationService;
        _logger = logger;
    }

    /// <summary>
    /// Simula um investimento com base no capital aplicado, quantidade de meses e produto escolhido.
    /// </summary>
    /// <param name="request">Um objeto contendo o userId, valor investido, quantidade de meses e uma lista de productIds.</param>
    /// <returns>Retorna </returns>
    [HttpPost]
    [ProducesResponseType(typeof(SimulationResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> SimulateInvestment([FromBody] SimulationRequestDto request)
    {
        try
        {
            var result = await _simulationService.SimulateInvestmentAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid simulation request for user {UserId}", request.UserId);
            return BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User profile not found for simulation");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating investment for user {UserId}", request.UserId);
            return StatusCode(500, "Erro interno ao processar simulação");
        }
    }

    /// <summary>
    /// Recupera a lista de simulações de um usuário a partir de seu userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<SimulationHistoryDto>), 200)]
    [Authorize]
    public async Task<IActionResult> GetUserSimulations(Guid userId)
    {
        try
        {
            var simulations = await _simulationService.GetUserSimulationsAsync(userId);
            return Ok(simulations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulations for user {UserId}", userId);
            return StatusCode(500, "Erro interno ao buscar simulações");
        }
    }

    /// <summary>
    /// Recupera uma simulação de investimento a partir do id da simulação
    /// </summary>
    /// <param name="simulationId"></param>
    /// <returns></returns>
    [HttpGet("{simulationId}")]
    [ProducesResponseType(typeof(SimulationResultDto), 200)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> GetSimulation(Guid simulationId)
    {
        try
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(simulationId);

            if (simulation == null)
                return NotFound("Simulação não encontrada");

            return Ok(simulation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulation {SimulationId}", simulationId);
            return StatusCode(500, "Erro interno ao buscar simulação");
        }
    }
}
