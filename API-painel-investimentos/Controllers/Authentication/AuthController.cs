using API_painel_investimentos.Controllers.User;
using API_painel_investimentos.DTO.Authentication;
using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Services.Authentication.Interfaces;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public AuthController(
        IJwtService jwtService, 
        IAuthService authService,
        IUserService userService,
        ILogger<UsersController> logger
    )
    {
        _jwtService = jwtService;
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // Validar request
            if (string.IsNullOrWhiteSpace(request.Password) ||
                (string.IsNullOrWhiteSpace(request.Cpf) && string.IsNullOrWhiteSpace(request.Email)))
            {
                return BadRequest("CPF ou Email e senha são obrigatórios");
            }

            var result = await _authService.AuthenticateAsync(request);

            if (result == null)
            {
                _logger.LogWarning("Login failed for CPF: {Cpf} or Email: {Email}",
                    request.Cpf, request.Email);
                return Unauthorized("CPF/Email ou senha incorretos");
            }

            _logger.LogInformation("Login successful for user {UserId}", result.UserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for CPF: {Cpf} or Email: {Email}",
                request.Cpf, request.Email);
            return StatusCode(500, "Erro interno durante o login");
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(TokenValidationResponseDto), 200)]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token é obrigatório");

            var result = await _authService.ValidateTokenAsync(token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Erro interno ao validar token");
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token é obrigatório");

            var validationResult = await _authService.ValidateTokenAsync(token);
            if (!validationResult.IsValid || !validationResult.UserId.HasValue)
                return Unauthorized("Token inválido ou expirado");

            var user = await _userService.GetUserByIdAsync(validationResult.UserId.Value);
            if (user == null)
                return Unauthorized("Token inválido ou expirado");

            // Buscar entidade completa do usuário para gerar novo token
            //var userEntity = await _authService.GetUserFromTokenAsync(token);
            //if (userEntity == null)
            //    return Unauthorized("Usuário não encontrado");

            //var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var newToken = _authService.GenerateToken(new UserEntity(
                user.Name,
                user.Cpf,
                user.Email,
                "" // Password não é necessário para gerar token
            ));

            var expiresAt = DateTime.UtcNow.AddMinutes(60); // Ajuste conforme configuração

            var response = new LoginResponseDto(
                user.UserId,
                user.Name,
                user.Email,
                newToken,
                expiresAt
            );

            _logger.LogInformation("Token refreshed for user {UserId}", user.UserId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "Erro interno ao renovar token");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("user-info")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            // Extrair token do header Authorization
            var token = ExtractTokenFromHeader();
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token não fornecido");

            var validationResult = await _authService.ValidateTokenAsync(token);
            if (!validationResult.IsValid || !validationResult.UserId.HasValue)
                return Unauthorized("Token inválido ou expirado");

            var user = await _authService.GetUserFromTokenAsync(token);
            if (user == null)
                return Unauthorized("Token inválido ou expirado");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info from token");
            return StatusCode(500, "Erro interno ao obter informações do usuário");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string? ExtractTokenFromHeader()
    {
        var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            return null;

        return authorizationHeader.Substring("Bearer ".Length).Trim();
    }
}
