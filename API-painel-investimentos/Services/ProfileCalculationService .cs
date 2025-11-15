using API_painel_investimentos.DTO;
using API_painel_investimentos.Services.Interfaces;

namespace API_painel_investimentos.Services;

public class ProfileCalculationService : IProfileCalculationService
{
    private readonly Dictionary<string, (int Min, int Max)> _profileRanges = new()
    {
        ["Conservative"] = (0, 30),
        ["Moderate"] = (31, 70),
        ["Aggressive"] = (71, 100)
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="answers"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public (string ProfileType, int Score) CalculateProfile(List<QuestionAnswer> answers)
    {
        if (!answers.Any())
            throw new ArgumentException("At least one answer is required");

        var totalScore = answers.Sum(a => a.Score);
        var profileType = DetermineProfileType(totalScore);

        return (profileType, totalScore);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    private string DetermineProfileType(int score)
    {
        foreach (var range in _profileRanges)
        {
            if (score >= range.Value.Min && score <= range.Value.Max)
                return range.Key;
        }

        // Fallback to conservative if score is outside expected ranges
        return "Conservative";
    }
}
