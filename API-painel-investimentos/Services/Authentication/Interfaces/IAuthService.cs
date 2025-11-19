using API_painel_investimentos.DTO.Authentication;
using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Models.User;

namespace API_painel_investimentos.Services.Authentication.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
    string GenerateToken(UserEntity user);
    Task<TokenValidationResponseDto> ValidateTokenAsync(string token);
    Task<UserResponseDto?> GetUserFromTokenAsync(string token);
}
