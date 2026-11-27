using AuthLab.Provider.Data;
using AuthLab.Provider.DTOs;
using AuthLab.Provider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace AuthLab.Provider.Services;

public class AuthorizationService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthorizationService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        _db = db;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<string?> CreateCodeAsync(
        string userId,
        string clientId,
        string redirectUri,
        string codeChallenge)
    {
        var client = await _db.OAuthClients
    .SingleOrDefaultAsync(c =>
        c.ClientId == clientId &&
        c.IsActive &&
        c.RedirectUris.Any(r => r.RedirectUri == redirectUri));

        if (client is null)
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        var rawCode = WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(32));

        var codeHash = WebEncoders.Base64UrlEncode(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawCode)));

        _db.AuthorizationCodes.Add(new AuthorizationCode
        {
            CodeHash = codeHash,
            UserId = user.Id,
            OAuthClientId = client.Id,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(2),
            IsUsed = false
        });

        await _db.SaveChangesAsync();

        return rawCode;
    }

    public async Task<TokenResponse?> ExchangeCodeAsync(
        TokenRequest request)
    {
        if (request.GrantType != "authorization_code")
            return null;

        var client = await _db.OAuthClients
    .SingleOrDefaultAsync(c =>
        c.ClientId == request.ClientId &&
        c.IsActive &&
        c.RedirectUris.Any(r => r.RedirectUri == request.RedirectUri));

        if (client is null)
            return null;

        var codeHash = WebEncoders.Base64UrlEncode(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(request.Code)));

        var authorizationCode = await _db.AuthorizationCodes
            .SingleOrDefaultAsync(c =>
                c.CodeHash == codeHash &&
                c.OAuthClientId == client.Id &&
                c.RedirectUri == request.RedirectUri &&
                !c.IsUsed &&
                c.ExpiresAtUtc > DateTime.UtcNow);

        if (authorizationCode is null)
            return null;

        // PKCE S256: hash the verifier and compare it
        // with the challenge saved during authorization.
        var actualChallenge = WebEncoders.Base64UrlEncode(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(request.CodeVerifier)));

        var actualBytes = Encoding.ASCII.GetBytes(actualChallenge);
        var expectedBytes = Encoding.ASCII.GetBytes(
            authorizationCode.CodeChallenge);

        if (actualBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(
                actualBytes, expectedBytes))
        {
            return null;
        }

        // Conditional update prevents two concurrent exchanges
        // from successfully consuming the same code.
        var updated = await _db.AuthorizationCodes
            .Where(c =>
                c.Id == authorizationCode.Id &&
                !c.IsUsed &&
                c.ExpiresAtUtc > DateTime.UtcNow)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(c => c.IsUsed, true));

        if (updated != 1)
            return null;

        var user = await _userManager.FindByIdAsync(
            authorizationCode.UserId.ToString());

        if (user is null)
            return null;

        var accessToken = await _tokenService.CreateAccessTokenAsync(
            user, _userManager);

        return new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 600
        };
    }
}