using API_painel_investimentos.DTO;
using API_painel_investimentos.Models;
using API_painel_investimentos.Repositories;
using API_painel_investimentos.Services.Interfaces;

namespace API_painel_investimentos.Services;

public class CalculateProfileCommandHandler : IRequestHandler<CalculateProfileCommand, ProfileResultDto>
{
    private readonly IInvestorProfileRepository _profileRepository;
    private readonly IProfileQuestionRepository _questionRepository;
    private readonly IProfileCalculationService _calculationService;

    public CalculateProfileCommandHandler(
        IInvestorProfileRepository profileRepository,
        IProfileQuestionRepository questionRepository,
        IProfileCalculationService calculationService)
    {
        _profileRepository = profileRepository;
        _questionRepository = questionRepository;
        _calculationService = calculationService;
    }

    public async Task<ProfileResultDto> Handle(CalculateProfileCommand request, CancellationToken cancellationToken)
    {
        // Validate all questions and answers
        var validatedAnswers = await ValidateAndMapAnswers(request.Answers);

        // Calculate profile
        var (profileType, score) = _calculationService.CalculateProfile(validatedAnswers);

        // Get or create profile
        var existingProfile = await _profileRepository.GetByUserIdAsync(request.UserId);

        if (existingProfile != null)
        {
            existingProfile.UpdateProfile(profileType, score);
            await _profileRepository.UpdateAsync(existingProfile);
        }
        else
        {
            existingProfile = new InvestorProfile(request.UserId, profileType, score, DateTime.UtcNow);
            await _profileRepository.CreateAsync(existingProfile);
        }

        // Map to detailed DTO
        return await MapToProfileResultDto(existingProfile, request.Answers);
    }

    private async Task<List<QuestionAnswer>> ValidateAndMapAnswers(List<UserAnswerDto> userAnswers)
    {
        var validatedAnswers = new List<QuestionAnswer>();

        foreach (var userAnswer in userAnswers)
        {
            var answerOption = await _questionRepository.GetAnswerOptionByIdAsync(userAnswer.AnswerOptionId);
            if (answerOption == null)
                throw new ArgumentException($"Invalid answer option ID: {userAnswer.AnswerOptionId}");

            validatedAnswers.Add(new QuestionAnswer(
                userAnswer.QuestionId,
                userAnswer.AnswerOptionId,
                answerOption.Score
            ));
        }

        return validatedAnswers;
    }

    private async Task<ProfileResultDto> MapToProfileResultDto(InvestorProfile profile, List<UserAnswerDto> userAnswers)
    {
        var answerDetails = new List<UserAnswerDetailDto>();

        foreach (var userAnswer in userAnswers)
        {
            var question = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
            var answerOption = await _questionRepository.GetAnswerOptionByIdAsync(userAnswer.AnswerOptionId);

            answerDetails.Add(new UserAnswerDetailDto(
                question?.QuestionText ?? "Unknown",
                answerOption?.OptionText ?? "Unknown",
                question?.Weight ?? 0,
                answerOption?.Score ?? 0
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
