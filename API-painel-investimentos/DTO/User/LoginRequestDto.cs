namespace API_painel_investimentos.DTO.User;

public record LoginRequestDto(
        string Cpf,
        string Password
);