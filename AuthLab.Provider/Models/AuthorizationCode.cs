namespace AuthLab.Provider.Models;

public class AuthorizationCode
{
    public Guid Id { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid OAuthClientId { get; set; }

    public string RedirectUri { get; set; } = string.Empty;

    public string CodeChallenge { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }
}