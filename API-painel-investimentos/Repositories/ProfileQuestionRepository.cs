using API_painel_investimentos.Infrastructure.Data;
using API_painel_investimentos.Models;
using API_painel_investimentos.Repositories.Interfaces;

namespace API_painel_investimentos.Repositories;

public class ProfileQuestionRepository : IProfileQuestionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProfileQuestionRepository> _logger;

    public ProfileQuestionRepository(
        ApplicationDbContext context,
        ILogger<ProfileQuestionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProfileQuestion>> GetActiveQuestionsAsync()
    {
        try
        {
            return await _context.ProfileQuestions
                .Include(pq => pq.AnswerOptions)
                .Where(pq => pq.IsActive)
                .OrderBy(pq => pq.Order)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active questions");
            throw;
        }
    }

    public async Task<ProfileQuestion?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.ProfileQuestions
                .Include(pq => pq.AnswerOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(pq => pq.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting question by ID {QuestionId}", id);
            throw;
        }
    }

    public async Task<QuestionAnswerOption?> GetAnswerOptionByIdAsync(Guid optionId)
    {
        try
        {
            return await _context.QuestionAnswerOptions
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == optionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting answer option by ID {OptionId}", optionId);
            throw;
        }
    }

    public async Task<bool> QuestionExistsAsync(Guid questionId)
    {
        try
        {
            return await _context.ProfileQuestions
                .AnyAsync(pq => pq.Id == questionId && pq.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if question exists {QuestionId}", questionId);
            throw;
        }
    }
}
