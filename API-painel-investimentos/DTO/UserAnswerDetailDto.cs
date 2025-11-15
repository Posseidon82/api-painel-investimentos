namespace API_painel_investimentos.DTO;

public record UserAnswerDetailDto(
        string QuestionText,
        string SelectedOption,
        int QuestionWeight,
        int OptionScore
);