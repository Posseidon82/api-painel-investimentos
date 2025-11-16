using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Services.Simulation.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Simulation
{
    // API/Controllers/StatsController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly ISimulationStatsService _statsService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(
            ISimulationStatsService statsService,
            ILogger<StatsController> logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        [HttpGet("products/daily")]
        [ProducesResponseType(typeof(ProductStatsResponseDto), 200)]
        public async Task<IActionResult> GetProductDailyStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? productId = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var request = new ProductStatsRequestDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProductId = productId,
                    Category = category
                };

                var result = await _statsService.GetProductDailyStatsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product daily stats");
                return StatusCode(500, "Erro interno ao gerar estatísticas");
            }
        }

        [HttpGet("products/top")]
        [ProducesResponseType(typeof(List<ProductDailyStatsDto>), 200)]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int topCount = 10)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var result = await _statsService.GetTopProductsAsync(start, end, topCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                return StatusCode(500, "Erro interno ao buscar produtos mais simulados");
            }
        }

        [HttpGet("products/{productId}/daily")]
        [ProducesResponseType(typeof(ProductStatsResponseDto), 200)]
        public async Task<IActionResult> GetProductStatsById(
            Guid productId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var request = new ProductStatsRequestDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProductId = productId
                };

                var result = await _statsService.GetProductDailyStatsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for product {ProductId}", productId);
                return StatusCode(500, "Erro interno ao gerar estatísticas do produto");
            }
        }
    }
}
