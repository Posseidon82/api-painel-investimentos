namespace API_painel_investimentos.DTO.User;

public record UserResponseDto(
        Guid UserId,
        string Name,
        string Cpf,
        string Email,
        bool IsActive,
        DateTime CreatedAt
);