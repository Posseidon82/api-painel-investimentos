using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_painel_investimentos.Controllers.User;

// API/Controllers/UsersController.cs
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma conta de usuário para dar acesso a plataforma de investimentos
    /// </summary>
    /// <param name="request">Um objeto <see cref="CreateUserRequestDto"/> que contém os dados necessários 
    /// para a criação da conta: nome, cpf, email e password.</param>
    /// <returns>Retorna um objeto <see cref="CreateUserResponseDto"/> com os dados de: userId, nome, cpf, email 
    /// e data de criação</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            var result = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { userId = result.UserId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid user creation request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Erro interno ao criar usuário");
        }
    }


    /// <summary>
    /// Recupera alguns dados da conta do usuário através de pesquisa pelo userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound("Usuário não encontrado");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return StatusCode(500, "Erro interno ao buscar usuário");
        }
    }

    /// <summary>
    /// Recupera alguns dados da conta do usuário através de pesquisa pelo cpf
    /// </summary>
    /// <param name="cpf"></param>
    /// <returns></returns>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> GetUserByCpf(string cpf)
    {
        try
        {
            var user = await _userService.GetUserByCpfAsync(cpf);

            if (user == null)
                return NotFound("Usuário não encontrado");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by CPF: {Cpf}", cpf);
            return StatusCode(500, "Erro interno ao buscar usuário");
        }
    }

    /// <summary>
    /// Recupera alguns dados da conta do usuário através de pesquisa pelo email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound("Usuário não encontrado");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Email: {Email}", email);
            return StatusCode(500, "Erro interno ao buscar usuário");
        }
    }

    /// <summary>
    /// Verifica se a conta do usuário existe.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("check-exists")]
    //[ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(CheckUserExistsResponseDto), 200)]
    [ProducesResponseType(400)]
    [Authorize]
    public async Task<IActionResult> CheckUserExists([FromBody] CheckUserExistsRequestDto request)
    //public async Task<IActionResult> CheckUserExists([FromBody] CreateUserRequestDto request)
    {
        try
        {
            //var exists = await _userService.UserExistsAsync(request.Cpf, request.Email);
            var exists = await _userService.CheckUserExistsAsync(request);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists");
            return StatusCode(500, "Erro interno ao verificar usuário");
        }
    }

    /// <summary>
    /// Atualiza os dados da conta do usuário.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            await _userService.UpdateUserAsync(userId, request);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return StatusCode(500, "Erro interno ao atualizar usuário");
        }
    }

    /// <summary>
    /// Modifica a password da conta do usuário.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{userId}/change-password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            await _userService.ChangePasswordAsync(userId, request);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return StatusCode(500, "Erro interno ao alterar senha");
        }
    }
}
