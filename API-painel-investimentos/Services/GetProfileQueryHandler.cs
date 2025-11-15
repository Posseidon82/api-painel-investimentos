using API_painel_investimentos.DTO;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Repositories.Interfaces;

namespace API_painel_investimentos.Services;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileResultDto>
{
    private readonly IInvestorProfileRepository _profileRepository;
    private readonly IProfileQuestionRepository _questionRepository;

    public GetProfileQueryHandler(
        IInvestorProfileRepository profileRepository,
        IProfileQuestionRepository questionRepository)
    {
        _profileRepository = profileRepository;
        _questionRepository = questionRepository;
    }

    public async Task<ProfileResultDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId);

        if (profile == null)
            throw new NotFoundException($"Profile not found for user {request.UserId}");

        var answerDetails = new List<UserAnswerDetailDto>();

        foreach (var answer in profile.Answers)
        {
            answerDetails.Add(new UserAnswerDetailDto(
                answer.Question.QuestionText,
                answer.SelectedOption.OptionText,
                answer.Question.Weight,
                answer.SelectedOption.Score
            ));
        }

        return new ProfileResultDto(
            profile.UserId,
            profile.ProfileType,
            profile.Score,
            profile.CalculatedAt,
            answerDetails
        );
    }
}
