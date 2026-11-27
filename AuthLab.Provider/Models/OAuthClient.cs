namespace AuthLab.Provider.Models;

public class OAuthClient
{
    public Guid Id { get; set; }

    public string ClientId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;


    public bool IsActive { get; set; } = true;

    public ICollection<OAuthRedirectUri> RedirectUris { get; set; }
        = new List<OAuthRedirectUri>();
}