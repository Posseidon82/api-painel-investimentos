using API_painel_investimentos.DTO.Authentication;
using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Models.Authentication;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Services.Authentication.Interfaces;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_painel_investimentos.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("Authentication attempt for CPF: {Cpf} or Email: {Email}",
                request.Cpf, request.Email);

            // Buscar usuário por credenciais
            var user = await _userService.GetUserByCredentialsAsync(request.Cpf, request.Email, request.Password);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for CPF: {Cpf} or Email: {Email}",
                    request.Cpf, request.Email);
                return null;
            }

            // Gerar token
            var userEntity = await _userService.GetUserEntityByIdAsync(user.UserId);
            if (userEntity == null) return null;

            var token = GenerateToken(userEntity);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            _logger.LogInformation("Authentication successful for user {UserId}", user.UserId);

            return new LoginResponseDto(
                user.UserId,
                user.Name,
                user.Email,
                token,
                expiresAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for CPF: {Cpf} or Email: {Email}",
                request.Cpf, request.Email);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("cpf", user.Cpf),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<TokenValidationResponseDto> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var nameClaim = principal.FindFirst(ClaimTypes.Name);
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            var expiresClaim = validatedToken.ValidTo;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new TokenValidationResponseDto(IsValid: false);
            }

            // Verificar se o usuário ainda existe e está ativo
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return new TokenValidationResponseDto(IsValid: false);
            }

            return new TokenValidationResponseDto(
                IsValid: true,
                UserId: userId,
                Name: nameClaim?.Value,
                Email: emailClaim?.Value,
                ExpiresAt: expiresClaim
            );
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new TokenValidationResponseDto(IsValid: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return new TokenValidationResponseDto(IsValid: false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<UserResponseDto?> GetUserFromTokenAsync(string token)
    {
        var validationResult = await ValidateTokenAsync(token);

        if (!validationResult.IsValid || !validationResult.UserId.HasValue)
            return null;

        return await _userService.GetUserByIdAsync(validationResult.UserId.Value);
    }
}
