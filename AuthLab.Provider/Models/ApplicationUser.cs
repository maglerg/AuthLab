using Microsoft.AspNetCore.Identity;

namespace AuthLab.Provider.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}