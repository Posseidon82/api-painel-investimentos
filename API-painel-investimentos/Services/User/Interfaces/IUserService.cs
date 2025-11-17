using API_painel_investimentos.DTO.User;

namespace API_painel_investimentos.Services.User.Interfaces;

public interface IUserService
{
    Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserResponseDto?> GetUserByIdAsync(Guid userId);
    Task<UserResponseDto?> GetUserByCpfAsync(string cpf);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string cpf, string email);
    Task<bool> ValidatePasswordAsync(Guid userId, string password);
    Task UpdateUserAsync(Guid userId, UpdateUserRequestDto request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}
