namespace API_painel_investimentos.DTO.User;

public record CheckUserExistsResponseDto(
        bool Exists,
        bool IsValidCredentials,
        Guid? UserId = null,
        string? Message = null
);