
using AuthLab.Provider.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthLab.Provider.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateAccessTokenAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager)
    {
        var privateKeyPath = _configuration["Jwt:PrivateKeyPath"]
            ?? throw new InvalidOperationException(
                "JWT private key path is not configured.");

        var keyId = _configuration["Jwt:KeyId"]
            ?? throw new InvalidOperationException(
                "JWT key ID is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        if (!File.Exists(privateKeyPath))
        {
            throw new InvalidOperationException(
                "JWT private key file was not found.");
        }

        using var rsa = RSA.Create();

        var privateKey = File.ReadAllText(privateKeyPath);
        rsa.ImportFromPem(privateKey);

        var securityKey = new RsaSecurityKey(rsa)
        {
            KeyId = keyId
        };

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.RsaSha256);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("name", user.DisplayName)
        };

        claims.AddRange(
            roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        token.Header["kid"] = keyId;

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
