namespace API_painel_investimentos.DTO.User;

public record CreateUserRequestDto(
        string Name,
        string Cpf,
        string Email,
        string Password
);