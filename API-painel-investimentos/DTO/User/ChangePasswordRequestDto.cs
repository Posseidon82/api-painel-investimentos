namespace API_painel_investimentos.DTO.User;

public record ChangePasswordRequestDto(
        string CurrentPassword,
        string NewPassword
);