using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.User.Interfaces;
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

    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
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


    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
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


    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
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


    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(404)]
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


    [HttpPost("check-exists")]
    //[ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(CheckUserExistsResponseDto), 200)]
    [ProducesResponseType(400)]
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


    [HttpPost("authenticate")]
    [ProducesResponseType(typeof(UserResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AuthenticateUser([FromBody] CheckUserExistsRequestDto request)
    {
        try
        {
            var user = await _userService.GetUserByCredentialsAsync(request.Cpf, request.Email, request.Password);

            if (user == null)
                return Unauthorized("CPF/Email ou senha incorretos");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user");
            return StatusCode(500, "Erro interno ao autenticar usuário");
        }
    }


    [HttpPut("{userId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
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


    [HttpPut("{userId}/change-password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
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
