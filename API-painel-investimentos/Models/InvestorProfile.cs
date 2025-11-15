namespace API_painel_investimentos.Models;

/// <summary>
/// Represents an investment profile for a user, including their profile type, score, and associated metadata.
/// </summary>
/// <remarks>The <see cref="InvestorProfile"/> class encapsulates the details of a user's investment profile,
/// such as their profile type (e.g., "Conservative" or "Aggressive"), a score representing their investment
/// preferences, and timestamps for when the profile was calculated or last updated. It also includes a collection of
/// associated questions that may have been used to determine the profile.</remarks>
public class InvestorProfile : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvestorProfile"/> class with the specified user ID, profile type,
    /// score, and calculation date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the investment profile.</param>
    /// <param name="profileType">The type of investment profile, such as "Conservative" or "Aggressive".</param>
    /// <param name="score">The score representing the user's investment profile. Must be a non-negative integer.</param>
    /// <param name="calculatedAt">The date and time when the investment profile was calculated.</param>
    public InvestorProfile(Guid userId, string profileType, int score, DateTime calculatedAt)
    {
        UserId = userId;
        ProfileType = profileType;
        Score = score;
        CalculatedAt = calculatedAt;
    }

    public Guid UserId { get; private set; }
    public string ProfileType { get; private set; }
    public int Score { get; private set; }
    public DateTime CalculatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Propriedade de navegação para perguntas associadas ao perfil
    public virtual ICollection<ProfileAnswer> Answers { get; private set; } = new List<ProfileAnswer>();

    /// <summary>
    /// Updates the profile with the specified type and score.Método para atualizar o perfil
    /// </summary>
    /// <remarks>This method updates the profile's type and score, and sets the last updated timestamp to the
    /// current UTC time.</remarks>
    /// <param name="profileType">The type of the profile to set. Cannot be null or empty.</param>
    /// <param name="score">The score to assign to the profile. Must be a non-negative integer.</param>
    public void UpdateProfile(string profileType, int score)
    {
        ProfileType = profileType;
        Score = score;
        UpdatedAt = DateTime.UtcNow;
    }
}
