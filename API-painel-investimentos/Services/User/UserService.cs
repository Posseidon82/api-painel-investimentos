using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Repositories.User.Interfaces;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace API_painel_investimentos.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

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

    private bool IsValidCpf(string cpf)
    {
        // CPF validation logic (simplified for example)
        var normalized = NormalizeCpf(cpf);
        return normalized.Length == 11 && normalized.All(char.IsDigit);
    }

    private string NormalizeCpf(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

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
}
