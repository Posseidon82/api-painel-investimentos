using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models.Simulation;
using API_painel_investimentos.Repositories.Simulation.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_painel_investimentos.Repositories.Simulation;

public class SimulationRepository : ISimulationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SimulationRepository> _logger;

    public SimulationRepository(
        ApplicationDbContext context,
        ILogger<SimulationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InvestmentSimulation> AddAsync(InvestmentSimulation simulation)
    {
        try
        {
            await _context.InvestmentSimulations.AddAsync(simulation);
            await _context.SaveChangesAsync();
            return simulation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding investment simulation for user {UserId}", simulation.UserId);
            throw;
        }
    }

    public async Task<InvestmentSimulation?> GetByIdAsync(Guid simulationId)
    {
        try
        {
            return await _context.InvestmentSimulations
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == simulationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulation by ID {SimulationId}", simulationId);
            throw;
        }
    }

    public async Task<List<InvestmentSimulation>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.InvestmentSimulations
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SimulatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulations for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<InvestmentSimulation>> GetRecentSimulationsAsync(int count = 10)
    {
        try
        {
            return await _context.InvestmentSimulations
                .OrderByDescending(s => s.SimulatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent simulations");
            throw;
        }
    }

    public async Task<List<InvestmentSimulation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.InvestmentSimulations
                .Where(s => s.SimulatedAt >= startDate && s.SimulatedAt <= endDate)
                .OrderBy(s => s.SimulatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulations from {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<List<InvestmentSimulation>> GetByProductIdAsync(Guid productId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.InvestmentSimulations.AsQueryable();

            // Filtrar por produto (precisa verificar o JSON)
            query = query.Where(s => s.SimulationDetails.Contains(productId.ToString()));

            if (startDate.HasValue)
                query = query.Where(s => s.SimulatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SimulatedAt <= endDate.Value);

            return await query
                .OrderByDescending(s => s.SimulatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulations for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<int> GetSimulationCountByDateAsync(DateTime date)
    {
        try
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            return await _context.InvestmentSimulations
                .Where(s => s.SimulatedAt >= startOfDay && s.SimulatedAt <= endOfDay)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simulation count for date {Date}", date);
            throw;
        }
    }
}
