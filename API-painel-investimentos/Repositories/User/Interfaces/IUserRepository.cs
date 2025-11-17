using API_painel_investimentos.Models.User;

namespace API_painel_investimentos.Repositories.User.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> AddAsync(UserEntity user);
    Task<UserEntity?> GetByIdAsync(Guid userId);
    Task<UserEntity?> GetByCpfAsync(string cpf);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task UpdateAsync(UserEntity user);
    Task<bool> ExistsByCpfAsync(string cpf);
    Task<bool> ExistsByEmailAsync(string email);
}
