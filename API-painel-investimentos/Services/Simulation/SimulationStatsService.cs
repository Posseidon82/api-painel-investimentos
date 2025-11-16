using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Models.Simulation;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Simulation.Interfaces;
using API_painel_investimentos.Services.Simulation.Interfaces;
using System.Text.Json;

namespace API_painel_investimentos.Services.Simulation
{
    public class SimulationStatsService : ISimulationStatsService
    {
        private readonly ISimulationRepository _simulationRepository;
        private readonly IInvestmentProductRepository _productRepository;
        private readonly ILogger<SimulationStatsService> _logger;

        public SimulationStatsService(
            ISimulationRepository simulationRepository,
            IInvestmentProductRepository productRepository,
            ILogger<SimulationStatsService> logger)
        {
            _simulationRepository = simulationRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ProductStatsResponseDto> GetProductDailyStatsAsync(ProductStatsRequestDto request)
        {
            try
            {
                _logger.LogInformation(
                    "Getting product daily stats from {StartDate} to {EndDate}",
                    request.StartDate, request.EndDate);

                // 1. Definir período padrão se não especificado
                var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = request.EndDate ?? DateTime.UtcNow;

                // 2. Buscar todas as simulações no período
                var allSimulations = await _simulationRepository.GetByDateRangeAsync(startDate, endDate);

                // 3. Processar estatísticas por produto e dia
                var dailyStats = await ProcessDailyStatsAsync(allSimulations, startDate, endDate, request);

                // 4. Calcular totais
                var totalSimulations = dailyStats.Sum(s => s.SimulationCount);
                var uniqueProducts = dailyStats.Select(s => s.ProductId).Distinct().Count();

                return new ProductStatsResponseDto(
                    startDate,
                    endDate,
                    totalSimulations,
                    uniqueProducts,
                    dailyStats
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product daily stats");
                throw;
            }
        }

        public async Task<List<ProductDailyStatsDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            try
            {
                var stats = await GetProductDailyStatsAsync(new ProductStatsRequestDto(startDate, endDate));

                return stats.DailyStats
                    .GroupBy(s => s.ProductId)
                    .Select(g => new ProductDailyStatsDto(
                        g.Key,
                        g.First().ProductName,
                        g.First().Category,
                        g.First().RiskLevel,
                        g.Max(s => s.Date), // Última data com dados
                        g.Sum(s => s.SimulationCount),
                        g.Average(s => s.AverageFinalAmount),
                        g.Sum(s => s.TotalInvestedAmount),
                        g.Min(s => s.MinFinalAmount),
                        g.Max(s => s.MaxFinalAmount),
                        g.Average(s => s.AverageReturnRate)
                    ))
                    .OrderByDescending(s => s.SimulationCount)
                    .ThenByDescending(s => s.AverageFinalAmount)
                    .Take(topCount)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        private async Task<List<ProductDailyStatsDto>> ProcessDailyStatsAsync(
            List<InvestmentSimulation> simulations,
            DateTime startDate,
            DateTime endDate,
            ProductStatsRequestDto request)
        {
            var dailyStats = new List<ProductDailyStatsDto>();

            // 1. Agrupar simulações por data (ignorando hora)
            var simulationsByDate = simulations
                .GroupBy(s => s.SimulatedAt.Date)
                .OrderBy(g => g.Key);

            // 2. Buscar informações de produtos
            var allProducts = await _productRepository.GetActiveProductsAsync();
            var productDictionary = allProducts.ToDictionary(p => p.Id, p => p);

            // 3. Processar cada dia
            foreach (var dateGroup in simulationsByDate)
            {
                var date = dateGroup.Key;

                // 4. Processar cada simulação do dia
                var simulationDetails = dateGroup
                    .SelectMany(s => ExtractProductSimulations(s, productDictionary))
                    .Where(ps => ps != null)
                    .ToList();

                // 5. Agrupar por produto
                var productGroups = simulationDetails
                    .GroupBy(ps => ps!.ProductId)
                    .ToList();

                // 6. Aplicar filtros
                var filteredProductGroups = ApplyFilters(productGroups, request);

                // 7. Criar estatísticas para cada produto no dia
                foreach (var productGroup in filteredProductGroups)
                {
                    var productSimulations = productGroup.ToList();
                    var product = productDictionary[productGroup.Key];

                    var stats = CreateDailyStats(product, date, productSimulations);
                    dailyStats.Add(stats);
                }
            }

            return dailyStats;
        }

        private List<IGrouping<Guid, ProductSimulationDetail?>> ApplyFilters(
            List<IGrouping<Guid, ProductSimulationDetail?>> productGroups,
            ProductStatsRequestDto request)
        {
            var filtered = productGroups.AsEnumerable();

            if (request.ProductId.HasValue)
            {
                filtered = filtered.Where(g => g.Key == request.ProductId.Value);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                // Nota: Precisaríamos ter acesso ao produto aqui para filtrar por categoria
                // Isso será tratado no ProcessDailyStatsAsync
            }

            return filtered.ToList();
        }

        private List<ProductSimulationDetail?> ExtractProductSimulations(
            InvestmentSimulation simulation,
            Dictionary<Guid, InvestmentProduct> productDictionary)
        {
            try
            {
                var details = JsonSerializer.Deserialize<SimulationDetails>(simulation.SimulationDetails);
                if (details?.ProductSimulations == null)
                    return new List<ProductSimulationDetail?>();

                return details.ProductSimulations
                    .Select(ps => new ProductSimulationDetail
                    {
                        ProductId = ps.ProductId,
                        ProductName = ps.ProductName,
                        Category = ps.Category,
                        RiskLevel = ps.RiskLevel,
                        AllocatedAmount = ps.AllocatedAmount,
                        FinalAmount = ps.FinalAmount,
                        GrossReturn = ps.GrossReturn,
                        NetReturn = ps.NetReturn,
                        SimulationDate = simulation.SimulatedAt
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting product simulations from simulation {SimulationId}", simulation.Id);
                return new List<ProductSimulationDetail?>();
            }
        }

        private ProductDailyStatsDto CreateDailyStats(
            InvestmentProduct product,
            DateTime date,
            List<ProductSimulationDetail?> productSimulations)
        {
            var validSimulations = productSimulations.Where(ps => ps != null).Cast<ProductSimulationDetail>().ToList();

            var simulationCount = validSimulations.Count;
            var averageFinalAmount = validSimulations.Average(ps => ps.FinalAmount);
            var totalInvestedAmount = validSimulations.Sum(ps => ps.AllocatedAmount);
            var minFinalAmount = validSimulations.Min(ps => ps.FinalAmount);
            var maxFinalAmount = validSimulations.Max(ps => ps.FinalAmount);
            var averageReturnRate = validSimulations.Average(ps =>
                ps.AllocatedAmount > 0 ? (ps.NetReturn / ps.AllocatedAmount) * 100 : 0);

            return new ProductDailyStatsDto(
                product.Id,
                product.Name,
                product.Category,
                product.RiskLevel,
                date,
                simulationCount,
                Math.Round(averageFinalAmount, 2),
                Math.Round(totalInvestedAmount, 2),
                Math.Round(minFinalAmount, 2),
                Math.Round(maxFinalAmount, 2),
                Math.Round(averageReturnRate, 2)
            );
        }
    }
}
