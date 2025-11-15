using API_painel_investimentos.DTO;

public record CalculateProfileCommand(Guid UserId, List<UserAnswerDto> Answers) : IRequest<ProfileResultDto>;