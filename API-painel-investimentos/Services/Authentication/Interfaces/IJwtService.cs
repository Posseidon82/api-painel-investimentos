namespace API_painel_investimentos.Services.Authentication.Interfaces;

public interface IJwtService
{
    string GenerateToken(string username, string[] roles);
}
