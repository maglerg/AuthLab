using AuthLab.Provider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthLab.Provider.Data;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<OAuthClient> OAuthClients => Set<OAuthClient>();

    public DbSet<AuthorizationCode> AuthorizationCodes
        => Set<AuthorizationCode>();

    public DbSet<OAuthRedirectUri> OAuthRedirectUris
        => Set<OAuthRedirectUri>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OAuthClient>()
            .HasIndex(c => c.ClientId)
            .IsUnique();

        builder.Entity<AuthorizationCode>()
            .HasIndex(c => c.CodeHash)
            .IsUnique();

        builder.Entity<OAuthRedirectUri>()
            .HasIndex(r => new { r.OAuthClientId, r.RedirectUri })
            .IsUnique();

        builder.Entity<OAuthRedirectUri>()
            .HasOne(r => r.OAuthClient)
            .WithMany(c => c.RedirectUris)
            .HasForeignKey(r => r.OAuthClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OAuthClient>()
            .Property(c => c.ClientId)
            .HasMaxLength(100);

        builder.Entity<AuthorizationCode>()
            .Property(c => c.CodeHash)
            .HasMaxLength(128);

        builder.Entity<AuthorizationCode>()
            .Property(c => c.CodeChallenge)
            .HasMaxLength(128);

        builder.Entity<OAuthRedirectUri>()
            .Property(r => r.RedirectUri)
            .HasMaxLength(2048);
    }
}