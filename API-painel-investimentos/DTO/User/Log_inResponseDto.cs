namespace API_painel_investimentos.DTO.User;

public record Log_inResponseDto(
        Guid UserId,
        string Name,
        string Email,
        string Token
);