namespace API_painel_investimentos.DTO.Profile;

public record UserAnswerDetailDto(
        string QuestionText,
        string SelectedOption,
        int QuestionWeight,
        int OptionScore
);