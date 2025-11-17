namespace API_painel_investimentos.DTO.User;

public record LoginResponseDto(
        Guid UserId,
        string Name,
        string Email,
        string Token
);