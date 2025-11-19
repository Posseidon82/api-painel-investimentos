namespace API_painel_investimentos.DTO.User;
public record CheckUserExistsRequestDto(
        string Password, 
        string? Cpf = null,
        string? Email = null
);