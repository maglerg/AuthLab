namespace AuthLab.Provider.Models;

public class OAuthRedirectUri
{
    public Guid Id { get; set; }

    public Guid OAuthClientId { get; set; }

    public string RedirectUri { get; set; } = string.Empty;

    public OAuthClient OAuthClient { get; set; } = null!;
}