namespace StudyFlowPro.Models;

public sealed class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public int PasswordIterations { get; set; } = 120_000;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
}

public sealed class AccountStore
{
    public int SchemaVersion { get; set; } = 1;
    public List<UserAccount> Accounts { get; set; } = new();
}
