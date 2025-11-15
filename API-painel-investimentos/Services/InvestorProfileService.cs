using API_painel_investimentos.DTO;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models;
using API_painel_investimentos.Repositories.Interfaces;
using API_painel_investimentos.Services.Interfaces;

namespace API_painel_investimentos.Services;

public class InvestorProfileService : IInvestorProfileService
{
    private readonly IInvestorProfileRepository _profileRepository;
    private readonly IProfileQuestionRepository _questionRepository;
    private readonly IProfileCalculationService _calculationService;
    //private readonly IUnitOfWork _unitOfWork;
    private readonly InvestorProfileContext _context;
    private readonly ILogger<InvestorProfileService> _logger;

    public InvestorProfileService(
        IInvestorProfileRepository profileRepository,
        IProfileQuestionRepository questionRepository,
        IProfileCalculationService calculationService,
        //IUnitOfWork unitOfWork,
        InvestorProfileContext context,
        ILogger<InvestorProfileService> logger)
    {
        _profileRepository = profileRepository;
        _questionRepository = questionRepository;
        _calculationService = calculationService;
        //_unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userAnswers"></param>
    /// <returns></returns>
    public async Task<ProfileResultDto> CalculateProfileAsync(Guid userId, List<UserAnswerDto> userAnswers)
    {
        _logger.LogInformation("Calculating profile for user {UserId}", userId);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            // Validar respostas
            var validatedAnswers = await ValidateAnswersAsync(userAnswers);

            // Converte para o tipo esperado pelo cálculo
            var questionAnswers = ConvertToQuestionAnswers(validatedAnswers);

            // Calcular perfil
            var (profileType, score) = _calculationService.CalculateProfile(questionAnswers);

            // Salvar no banco
            var profile = await SaveOrUpdateProfileAsync(userId, profileType, score, userAnswers);

            // Mapear para DTO
            var result = await MapToProfileResultDtoAsync(profile, userAnswers);

            //await _unitOfWork.CommitAsync();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Profile calculated for user {UserId}: {ProfileType}", userId, profileType);

            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ProfileResultDto> GetUserProfileAsync(Guid userId)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId);

        if (profile == null)
            throw new NotFoundException($"Profile not found for user {userId}");

        return await MapToProfileResultDtoAsync(profile, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> ProfileExistsAsync(Guid userId)
    {
        return await _profileRepository.ExistsForUserAsync(userId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userAnswers"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<List<ValidatedAnswer>> ValidateAnswersAsync(List<UserAnswerDto> userAnswers)
    {
        var validatedAnswers = new List<ValidatedAnswer>();

        foreach (var userAnswer in userAnswers)
        {
            var question = await _questionRepository.GetByIdAsync(userAnswer.QuestionId);
            if (question == null)
                throw new ArgumentException($"Question not found: {userAnswer.QuestionId}");

            var answerOption = await _questionRepository.GetAnswerOptionByIdAsync(userAnswer.AnswerOptionId);
            if (answerOption == null || answerOption.QuestionId != userAnswer.QuestionId)
                throw new ArgumentException($"Invalid answer option: {userAnswer.AnswerOptionId} for question: {userAnswer.QuestionId}");

            validatedAnswers.Add(new ValidatedAnswer(question, answerOption));
        }

        return validatedAnswers;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="profileType"></param>
    /// <param name="score"></param>
    /// <param name="userAnswers"></param>
    /// <returns></returns>
    private async Task<InvestorProfile> SaveOrUpdateProfileAsync(Guid userId, string profileType, int score, List<UserAnswerDto> userAnswers)
    {
        var existingProfile = await _profileRepository.GetByUserIdAsync(userId);

        if (existingProfile != null)
        {
            // Atualizar perfil existente
            existingProfile.UpdateProfile(profileType, score);

            // Remover respostas antigas
            await _profileRepository.ClearProfileAnswersAsync(existingProfile.Id);

            // Adicionar novas respostas
            await AddProfileAnswersAsync(existingProfile.Id, userAnswers);

            await _profileRepository.UpdateAsync(existingProfile);
            return existingProfile;
        }
        else
        {
            // Criar novo perfil
            var newProfile = new InvestorProfile(userId, profileType, score, DateTime.UtcNow);
            await _profileRepository.CreateAsync(newProfile);

            // Adicionar respostas
            await AddProfileAnswersAsync(newProfile.Id, userAnswers);

            return newProfile;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="userAnswers"></param>
    /// <returns></returns>
    private async Task AddProfileAnswersAsync(Guid profileId, List<UserAnswerDto> userAnswers)
    {
        foreach (var userAnswer in userAnswers)
        {
            var profileAnswer = new ProfileAnswer(profileId, userAnswer.QuestionId, userAnswer.AnswerOptionId);
            await _profileRepository.AddProfileAnswerAsync(profileAnswer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="userAnswers"></param>
    /// <returns></returns>
    private async Task<ProfileResultDto> MapToProfileResultDtoAsync(InvestorProfile profile, List<UserAnswerDto>? userAnswers)
    {
        var answerDetails = new List<UserAnswerDetailDto>();

        if (userAnswers != null)
        {
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
        }
        else
        {
            // Carregar respostas do perfil existente
            var answers = await _profileRepository.GetProfileAnswersAsync(profile.Id);
            foreach (var answer in answers)
            {
                answerDetails.Add(new UserAnswerDetailDto(
                    answer.Question.QuestionText,
                    answer.SelectedOption.OptionText,
                    answer.Question.Weight,
                    answer.SelectedOption.Score
                ));
            }
        }

        return new ProfileResultDto(
            profile.UserId,
            profile.ProfileType,
            profile.Score,
            profile.CalculatedAt,
            answerDetails
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="validatedAnswers"></param>
    /// <returns></returns>
    private List<QuestionAnswer> ConvertToQuestionAnswers(List<ValidatedAnswer> validatedAnswers)
    {
        return validatedAnswers.Select(va => new QuestionAnswer(
            va.Question.Id,
            va.AnswerOption.Id,
            va.AnswerOption.Score
        )).ToList();
    }
}