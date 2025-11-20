using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Repositories.User.Interfaces;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace API_painel_investimentos.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ApplicationDbContext context,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating user for CPF: {Cpf}", request.Cpf);

            // 1. Validar dados
            ValidateUserRequest(request);

            // 2. Verificar se usuário já existe
            var existingUser = await _userRepository.GetByCpfAsync(request.Cpf) ??
                             await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("User already exists with CPF: {Cpf} or Email: {Email}", request.Cpf, request.Email);
                throw new ArgumentException("Usuário já cadastrado com este CPF ou E-mail");
            }

            // 3. Criar hash da senha
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // 4. Criar usuário
            var user = new UserEntity(
                request.Name.Trim(),
                NormalizeCpf(request.Cpf),
                request.Email.Trim().ToLower(),
                passwordHash
            );

            // 5. Salvar no banco
            await _userRepository.AddAsync(user);

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

            // 6. Retornar resposta
            return new CreateUserResponseDto(
                user.Id,
                user.Name,
                user.Cpf,
                user.Email,
                user.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user for CPF: {Cpf}", request.Cpf);
            throw;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : MapToUserResponseDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            throw;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<UserEntity?> GetUserEntityByIdAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user entity by ID: {UserId}", userId);
            throw;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cpf"></param>
    /// <returns></returns>
    public async Task<UserResponseDto?> GetUserByCpfAsync(string cpf)
    {
        try
        {
            var normalizedCpf = NormalizeCpf(cpf);
            var user = await _userRepository.GetByCpfAsync(normalizedCpf);
            return user == null ? null : MapToUserResponseDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by CPF: {Cpf}", cpf);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLower();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            return user == null ? null : MapToUserResponseDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cpf"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> UserExistsAsync(string cpf, string email)
    {
        try
        {
            var normalizedCpf = NormalizeCpf(cpf);
            var normalizedEmail = email.Trim().ToLower();

            var userByCpf = await _userRepository.GetByCpfAsync(normalizedCpf);
            var userByEmail = await _userRepository.GetByEmailAsync(normalizedEmail);

            return userByCpf != null || userByEmail != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists with CPF: {Cpf} and Email: {Email}", cpf, email);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            return _passwordHasher.VerifyPassword(password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task UpdateUserAsync(Guid userId, UpdateUserRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"Usuário não encontrado: {userId}");

            user.UpdateProfile(request.Name.Trim(), request.Email.Trim().ToLower());
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User updated successfully: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"Usuário não encontrado: {userId}");

            // Verificar senha atual
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                throw new ArgumentException("Senha atual incorreta");

            // Gerar novo hash e atualizar
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="ArgumentException"></exception>
    private void ValidateUserRequest(CreateUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(request.Cpf))
            throw new ArgumentException("CPF é obrigatório");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("E-mail é obrigatório");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Senha é obrigatória");

        if (request.Password.Length < 6)
            throw new ArgumentException("Senha deve ter pelo menos 6 caracteres");

        if (!IsValidEmail(request.Email))
            throw new ArgumentException("E-mail inválido");

        if (!IsValidCpf(request.Cpf))
            throw new ArgumentException("CPF inválido");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cpf"></param>
    /// <returns></returns>
    private bool IsValidCpf(string cpf)
    {
        // CPF validation logic (simplified for example)
        var normalized = NormalizeCpf(cpf);
        return normalized.Length == 11 && normalized.All(char.IsDigit);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cpf"></param>
    /// <returns></returns>
    private string NormalizeCpf(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private UserResponseDto MapToUserResponseDto(UserEntity user)
    {
        return new UserResponseDto(
            user.Id,
            user.Name,
            user.Cpf,
            user.Email,
            user.IsActive,
            user.CreatedAt
        );
    }

    /// <summary>
    /// Verifica se o usuário existe e se as credenciais são válidas
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<CheckUserExistsResponseDto> CheckUserExistsAsync(CheckUserExistsRequestDto request)
    {
        try
        {
            _logger.LogInformation("Checking user existence for CPF: {Cpf} or Email: {Email}",
                request.Cpf, request.Email);

            // Validar request
            var validationResult = ValidateCheckUserRequest(request);
            if (!validationResult.IsValid)
            {
                return new CheckUserExistsResponseDto(
                    Exists: false,
                    IsValidCredentials: false,
                    Message: validationResult.ErrorMessage
                );
            }

            // Buscar usuário por CPF ou Email
            var user = await FindUserByCpfOrEmailAsync(request.Cpf, request.Email);

            if (user == null)
            {
                _logger.LogInformation("User not found for CPF: {Cpf} or Email: {Email}",
                    request.Cpf, request.Email);

                return new CheckUserExistsResponseDto(
                    Exists: false,
                    IsValidCredentials: false,
                    Message: "Usuário não encontrado"
                );
            }

            // Validar senha
            var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isValidPassword)
            {
                _logger.LogWarning("Invalid password for user {UserId}", user.Id);

                return new CheckUserExistsResponseDto(
                    Exists: true,
                    IsValidCredentials: false,
                    UserId: user.Id,
                    Message: "Senha incorreta"
                );
            }

            _logger.LogInformation("User found and credentials are valid for user {UserId}", user.Id);

            return new CheckUserExistsResponseDto(
                Exists: true,
                IsValidCredentials: true,
                UserId: user.Id,
                Message: "Credenciais válidas"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user existence for CPF: {Cpf} or Email: {Email}",
                request.Cpf, request.Email);
            throw;
        }
    }


    /// <summary>
    /// Busca usuário por credenciais (CPF+senha ou Email+senha)
    /// </summary>
    /// <param name="cpf"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<UserResponseDto?> GetUserByCredentialsAsync(string? cpf, string? email, string password)
    {
        try
        {
            var user = await FindUserByCpfOrEmailAsync(cpf, email);

            if (user == null) return null;

            // Validar senha
            var isValidPassword = _passwordHasher.VerifyPassword(password, user.PasswordHash);

            return isValidPassword ? MapToUserResponseDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by credentials for CPF: {Cpf} or Email: {Email}", cpf, email);
            throw;
        }
    }


    /// <summary>
    /// Valida o request de verificação de usuário
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private (bool IsValid, string? ErrorMessage) ValidateCheckUserRequest(CheckUserExistsRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            return (false, "Senha é obrigatória");

        if (string.IsNullOrWhiteSpace(request.Cpf) && string.IsNullOrWhiteSpace(request.Email))
            return (false, "CPF ou Email é obrigatório");

        if (!string.IsNullOrWhiteSpace(request.Cpf) && !IsValidCpf(request.Cpf))
            return (false, "CPF inválido");

        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            return (false, "Email inválido");

        return (true, null);
    }


    /// <summary>
    /// Busca usuário por CPF ou Email
    /// </summary>
    /// <param name="cpf"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    private async Task<UserEntity?> FindUserByCpfOrEmailAsync(string? cpf, string? email)
    {
        if (!string.IsNullOrWhiteSpace(cpf))
        {
            var normalizedCpf = NormalizeCpf(cpf);
            var userByCpf = await _userRepository.GetByCpfAsync(normalizedCpf);
            if (userByCpf != null) return userByCpf;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLower();
            var userByEmail = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (userByEmail != null) return userByEmail;
        }

        return null;
    }
}
