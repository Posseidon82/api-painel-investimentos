using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Models.User;

namespace API_painel_investimentos.Services.User.Interfaces;

public interface IUserService
{
    Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request);

    Task<UserResponseDto?> GetUserByIdAsync(Guid userId);
    Task<UserEntity?> GetUserEntityByIdAsync(Guid userId); //Retorna a entidade do usuário (para autenticação)

    Task<UserResponseDto?> GetUserByCpfAsync(string cpf);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);

    Task<CheckUserExistsResponseDto> CheckUserExistsAsync(CheckUserExistsRequestDto request);

    Task<bool> UserExistsAsync(string cpf, string email);

    Task<bool> ValidatePasswordAsync(Guid userId, string password);
    Task UpdateUserAsync(Guid userId, UpdateUserRequestDto request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

    Task<UserResponseDto?> GetUserByCredentialsAsync(string? cpf, string? email, string password);
}
