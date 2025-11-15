using API_painel_investimentos.DTO;

public record GetProfileQuery(Guid UserId) : IRequest<ProfileResultDto>;