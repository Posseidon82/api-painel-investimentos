namespace API_painel_investimentos.DTO.Authentication;

public record LoginRequestDto(
        string Password, 
        string? Cpf = null,
        string? Email = null
);