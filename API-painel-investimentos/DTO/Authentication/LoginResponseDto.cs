namespace API_painel_investimentos.DTO.Authentication;

public record LoginResponseDto(
        Guid UserId,
        string Name,
        string Email,
        string Token,
        DateTime ExpiresAt,
        string TokenType = "Bearer"
);