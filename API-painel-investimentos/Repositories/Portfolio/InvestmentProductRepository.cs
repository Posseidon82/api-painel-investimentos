using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_painel_investimentos.Repositories.Portfolio
{
    public class InvestmentProductRepository : IInvestmentProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvestmentProductRepository> _logger;

        public InvestmentProductRepository(
            ApplicationDbContext context,
            ILogger<InvestmentProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retorna os produtos de investimento ativos
        /// </summary>
        /// <returns></returns>
        public async Task<List<InvestmentProduct>> GetActiveProductsAsync()
        {
            try
            {
                return await _context.InvestmentProducts
                    .Where(ip => ip.IsActive)
                    .OrderBy(ip => ip.Category)
                    .ThenBy(ip => ip.RiskLevel)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active investment products");
                throw;
            }
        }


        /// <summary>
        /// Retorna os produtos de investimento com o mesmo productId e que estejam ativos
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<InvestmentProduct?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.InvestmentProducts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ip => ip.Id == id && ip.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting investment product by ID {ProductId}", id);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<InvestmentProduct>> GetByCategoryAsync(string category)
        {
            try
            {
                return await _context.InvestmentProducts
                    .Where(ip => ip.Category == category && ip.IsActive)
                    .OrderBy(ip => ip.RiskLevel)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting investment products by category {Category}", category);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="riskLevel"></param>
        /// <returns></returns>
        public async Task<List<InvestmentProduct>> GetByRiskLevelAsync(string riskLevel)
        {
            try
            {
                return await _context.InvestmentProducts
                    .Where(ip => ip.RiskLevel == riskLevel && ip.IsActive)
                    .OrderByDescending(ip => ip.ExpectedReturn)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting investment products by risk level {RiskLevel}", riskLevel);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetProfile"></param>
        /// <returns></returns>
        public async Task<List<InvestmentProduct>> GetByProfileAsync(string targetProfile)
        {
            try
            {
                return await _context.InvestmentProducts
                    .Where(ip => ip.TargetProfile.Contains(targetProfile) && ip.IsActive)
                    .OrderBy(ip => ip.RiskLevel)
                    .ThenByDescending(ip => (double)ip.ExpectedReturn)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting investment products by target profile {TargetProfile}", targetProfile);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task UpdateProductAsync(InvestmentProduct product)
        {
            try
            {
                _context.InvestmentProducts.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating investment product {ProductId}", product.Id);
                throw;
            }
        }
    }
}
