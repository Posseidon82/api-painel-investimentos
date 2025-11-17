namespace API_painel_investimentos.Models.User;

public class UserEntity : BaseEntity
{
    public UserEntity(
        string name,
        string cpf,
        string email,
        string passwordHash)
    {
        Name = name;
        Cpf = cpf;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Cpf { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }

    // Métodos
    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
        UpdateTimestamps();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdateTimestamps();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamps();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamps();
    }
}
