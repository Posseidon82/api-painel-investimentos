namespace API_painel_investimentos.DTO.Authentication;

public record TokenValidationResponseDto(
        bool IsValid,
        Guid? UserId = null,
        string? Name = null,
        string? Email = null,
        DateTime? ExpiresAt = null
);