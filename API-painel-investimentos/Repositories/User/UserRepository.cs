using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Repositories.User.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_painel_investimentos.Repositories.User;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        ApplicationDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserEntity> AddAsync(UserEntity user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user with CPF: {Cpf}", user.Cpf);
            throw;
        }
    }

    public async Task<UserEntity?> GetByIdAsync(Guid userId)
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserEntity?> GetByCpfAsync(string cpf)
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Cpf == cpf && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by CPF: {Cpf}", cpf);
            throw;
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Email: {Email}", email);
            throw;
        }
    }

    public async Task UpdateAsync(UserEntity user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> ExistsByCpfAsync(string cpf)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => u.Cpf == cpf && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists by CPF: {Cpf}", cpf);
            throw;
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists by Email: {Email}", email);
            throw;
        }
    }
}
