namespace API_painel_investimentos.DTO.User;

public record CreateUserResponseDto(
        Guid UserId,
        string Name,
        string Cpf,
        string Email,
        DateTime CreatedAt
);