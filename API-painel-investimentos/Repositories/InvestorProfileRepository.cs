using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models;
using API_painel_investimentos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_painel_investimentos.Repositories;

public class InvestorProfileRepository : IInvestorProfileRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvestorProfileRepository> _logger;

    public InvestorProfileRepository(
        ApplicationDbContext context,
        ILogger<InvestorProfileRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InvestorProfile?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.InvestorProfiles
                .Include(ip => ip.Answers)
                    .ThenInclude(a => a.Question)
                .Include(ip => ip.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting investor profile for user {UserId}", userId);
            throw;
        }
    }

    public async Task<InvestorProfile> CreateAsync(InvestorProfile profile)
    {
        try
        {
            await _context.InvestorProfiles.AddAsync(profile);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating investor profile for user {UserId}", profile.UserId);
            throw;
        }
    }

    public async Task UpdateAsync(InvestorProfile profile)
    {
        try
        {
            _context.InvestorProfiles.Update(profile);

            // Marcar como modificado para garantir o update
            _context.Entry(profile).State = EntityState.Modified;

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating investor profile {ProfileId}", profile.Id);
            throw;
        }
    }

    public async Task<bool> ExistsForUserAsync(Guid userId)
    {
        try
        {
            return await _context.InvestorProfiles
                .AnyAsync(ip => ip.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if profile exists for user {UserId}", userId);
            throw;
        }
    }

    public async Task AddProfileAnswerAsync(ProfileAnswer answer)
    {
        try
        {
            await _context.ProfileAnswers.AddAsync(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding profile answer for profile {ProfileId}", answer.ProfileId);
            throw;
        }
    }

    public async Task ClearProfileAnswersAsync(Guid profileId)
    {
        try
        {
            var answers = await _context.ProfileAnswers
                .Where(pa => pa.ProfileId == profileId)
                .ToListAsync();

            _context.ProfileAnswers.RemoveRange(answers);

            _logger.LogInformation("Cleared {Count} answers for profile {ProfileId}", answers.Count, profileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing profile answers for profile {ProfileId}", profileId);
            throw;
        }
    }

    public async Task<List<ProfileAnswer>> GetProfileAnswersAsync(Guid profileId)
    {
        try
        {
            return await _context.ProfileAnswers
                .Include(pa => pa.Question)
                .Include(pa => pa.SelectedOption)
                .Where(pa => pa.ProfileId == profileId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile answers for profile {ProfileId}", profileId);
            throw;
        }
    }
}
